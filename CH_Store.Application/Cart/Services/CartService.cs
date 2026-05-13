using CH_Store.Application.Cart.Command;
using CH_Store.Application.Cart.Command.CartCommands;
using CH_Store.Application.Cart.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CH_Store.Application.Cart.Services
{
     /// <summary>
     /// Serviciu pentru cosul de cumparaturi — orchestreaza Command Pattern.
     ///
     /// Responsabilitati:
     ///   • Incarca / salveaza CartData din IMemoryCache (TTL 4h sliding)
     ///   • Creeaza comenzile corespunzatoare fiecarei actiuni
     ///   • Deleaga executia catre ICommandInvoker (care gestioneaza stiva + istoricul)
     ///   • Expune Undo si GetHistory catre Controller
     ///
     /// CartData este stocat ca referinta in IMemoryCache.
     /// Comenzile tin referinta la CartData si o modifica in-place →
     /// Undo functioneaza cross-request fara re-incarcare din DB.
     /// </summary>
     public class CartService : ICartService
     {
          private readonly ICommandInvoker _invoker;
          private readonly IMemoryCache    _cache;

          private static readonly TimeSpan CartTtl = TimeSpan.FromHours(4);

          private static string CartKey(int userId) => $"cart:{userId}";

          public CartService(ICommandInvoker invoker, IMemoryCache cache)
          {
               _invoker = invoker;
               _cache   = cache;
          }

          // ── Operatii ─────────────────────────────────────────────────────────

          public async Task<CartResponse> AddItemAsync(int userId, CartItem item)
          {
               var cart    = LoadOrCreate(userId);
               var command = new AddItemToCartCommand(cart, item);
               await _invoker.ExecuteAsync(command, userId);
               Touch(userId, cart);
               return BuildResponse(userId, cart, command.Description);
          }

          public async Task<CartResponse> RemoveItemAsync(int userId, int productId)
          {
               var cart    = LoadOrCreate(userId);
               var command = new RemoveItemFromCartCommand(cart, productId);
               await _invoker.ExecuteAsync(command, userId);
               Touch(userId, cart);
               return BuildResponse(userId, cart, command.Description);
          }

          public async Task<CartResponse> UpdateQuantityAsync(int userId, int productId, int newQuantity)
          {
               var cart    = LoadOrCreate(userId);
               var command = new UpdateItemQuantityCommand(cart, productId, newQuantity);
               await _invoker.ExecuteAsync(command, userId);
               Touch(userId, cart);
               return BuildResponse(userId, cart, command.Description);
          }

          public async Task<CartResponse> ClearAsync(int userId)
          {
               var cart    = LoadOrCreate(userId);
               var command = new ClearCartCommand(cart);
               await _invoker.ExecuteAsync(command, userId);
               Touch(userId, cart);
               return BuildResponse(userId, cart, command.Description);
          }

          // ── Undo ─────────────────────────────────────────────────────────────

          public async Task<(bool Success, string Message, CartResponse Cart)> UndoAsync(int userId)
          {
               var cart = LoadOrCreate(userId);
               var (success, message) = await _invoker.UndoLastAsync(userId);
               Touch(userId, cart);   // reseteaza TTL — cartul a fost modificat in-place
               return (success, message, BuildResponse(userId, cart, message));
          }

          // ── Interogari ───────────────────────────────────────────────────────

          public CartResponse GetCart(int userId)
          {
               var cart = LoadOrCreate(userId);
               return BuildResponse(userId, cart, null);
          }

          public IReadOnlyList<CommandHistoryEntry> GetHistory(int userId)
               => _invoker.GetHistory(userId);

          public void DeleteCart(int userId)
          {
               _cache.Remove(CartKey(userId));
               _invoker.ClearHistory(userId);
          }

          // ── Helper ───────────────────────────────────────────────────────────

          private CartData LoadOrCreate(int userId)
               => _cache.GetOrCreate(CartKey(userId), e =>
               {
                    e.SlidingExpiration = CartTtl;
                    return new CartData { UserId = userId };
               })!;

          /// <summary>Reseteaza TTL-ul dupa modificare (CartData e acelasi obiect in cache).</summary>
          private void Touch(int userId, CartData cart)
               => _cache.Set(CartKey(userId), cart, new MemoryCacheEntryOptions
               {
                    SlidingExpiration = CartTtl
               });

          private CartResponse BuildResponse(int userId, CartData cart, string? lastOp)
               => CartResponse.From(cart, _invoker.GetUndoStackDepth(userId), lastOp);
     }
}
