using CH_Store.Domain.DTOs;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Cart.Interfaces
{
     /// <summary>
     /// Serviciu pentru gestionarea cosului de cumparaturi via Command Pattern.
     /// Toate operatiile de scriere sunt incapsulate in comenzi cu suport Undo.
     /// </summary>
     public interface ICartService
     {
          // ─── Operatii (fiecare creeaza si executa un ICommand) ───────────────

          Task<CartResponse> AddItemAsync(int userId, CartItem item);
          Task<CartResponse> RemoveItemAsync(int userId, int productId);
          Task<CartResponse> UpdateQuantityAsync(int userId, int productId, int newQuantity);
          Task<CartResponse> ClearAsync(int userId);

          // ─── Undo ────────────────────────────────────────────────────────────

          Task<(bool Success, string Message, CartResponse Cart)> UndoAsync(int userId);

          // ─── Interogari ──────────────────────────────────────────────────────

          CartResponse GetCart(int userId);
          IReadOnlyList<CommandHistoryEntry> GetHistory(int userId);
          void DeleteCart(int userId);
     }
}
