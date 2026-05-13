using CH_Store.Application.Cart.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     /// <summary>
     /// Controller pentru cosul de cumparaturi si comenzi via Command Pattern.
     ///
     /// Endpoints cos (Command Pattern — cart):
     ///   POST   /api/cart/{userId}/items          → AddItemToCartCommand
     ///   DELETE /api/cart/{userId}/items/{id}     → RemoveItemFromCartCommand
     ///   PATCH  /api/cart/{userId}/items/{id}     → UpdateItemQuantityCommand
     ///   DELETE /api/cart/{userId}                → ClearCartCommand
     ///   POST   /api/cart/{userId}/undo           → Undo ultima operatie cos
     ///   GET    /api/cart/{userId}                → Starea curenta a cosului
     ///   GET    /api/cart/{userId}/history        → Jurnalul comenzilor executate
     ///   DELETE /api/cart/{userId}/session        → Sterge cosul si istoricul
     ///
     /// Endpoints order commands:
     ///   POST   /api/cart/orders                  → PlaceOrderCommand
     ///   POST   /api/cart/orders/{id}/cancel      → CancelOrderCommand
     ///   PATCH  /api/cart/orders/{id}/status      → UpdateOrderStatusCommand
     ///   POST   /api/cart/orders/undo             → Undo ultima operatie order (in-scope)
     ///   GET    /api/cart/orders/session-history  → Istoric sesiune curenta
     /// </summary>
     [ApiController]
     [Route("api/[controller]")]
     public class CartController : ControllerBase
     {
          private readonly ICartService         _cartService;
          private readonly IOrderCommandService _orderCommandService;

          public CartController(ICartService cartService, IOrderCommandService orderCommandService)
          {
               _cartService         = cartService;
               _orderCommandService = orderCommandService;
          }

          // ════════════════════════════════════════════════════════════════════
          // COS DE CUMPARATURI — Command Pattern (cart)
          // ════════════════════════════════════════════════════════════════════

          // ── GET /api/cart/{userId} ───────────────────────────────────────────
          [HttpGet("{userId:int}")]
          public IActionResult GetCart(int userId)
          {
               var cart = _cartService.GetCart(userId);
               return Ok(cart);
          }

          // ── POST /api/cart/{userId}/items ────────────────────────────────────
          /// <summary>
          /// Adauga un produs in cos.
          /// Daca produsul exista deja, cantitatea este incrementata.
          /// Suporta Undo via POST /api/cart/{userId}/undo.
          /// </summary>
          [HttpPost("{userId:int}/items")]
          public async Task<IActionResult> AddItem(int userId, [FromBody] CartItemRequest request)
          {
               if (request.Quantity <= 0)
                    return BadRequest("Cantitatea trebuie sa fie cel putin 1.");

               var cart = await _cartService.AddItemAsync(userId, new CartItem
               {
                    ProductId   = request.ProductId,
                    ProductName = request.ProductName,
                    Price       = request.Price,
                    Quantity    = request.Quantity
               });

               return Ok(cart);
          }

          // ── DELETE /api/cart/{userId}/items/{productId} ─────────────────────
          /// <summary>
          /// Sterge un produs din cos.
          /// Suporta Undo via POST /api/cart/{userId}/undo.
          /// </summary>
          [HttpDelete("{userId:int}/items/{productId:int}")]
          public async Task<IActionResult> RemoveItem(int userId, int productId)
          {
               var cart = await _cartService.RemoveItemAsync(userId, productId);
               return Ok(cart);
          }

          // ── PATCH /api/cart/{userId}/items/{productId} ───────────────────────
          /// <summary>
          /// Actualizeaza cantitatea unui produs.
          /// Quantity = 0 → sterge produsul din cos.
          /// Suporta Undo via POST /api/cart/{userId}/undo.
          /// </summary>
          [HttpPatch("{userId:int}/items/{productId:int}")]
          public async Task<IActionResult> UpdateQuantity(
               int userId,
               int productId,
               [FromBody] UpdateCartItemRequest request)
          {
               if (request.Quantity < 0)
                    return BadRequest("Cantitatea nu poate fi negativa.");

               var cart = await _cartService.UpdateQuantityAsync(userId, productId, request.Quantity);
               return Ok(cart);
          }

          // ── DELETE /api/cart/{userId} ────────────────────────────────────────
          /// <summary>
          /// Goleste complet cosul.
          /// Suporta Undo via POST /api/cart/{userId}/undo (restaureaza toate produsele).
          /// </summary>
          [HttpDelete("{userId:int}")]
          public async Task<IActionResult> ClearCart(int userId)
          {
               var cart = await _cartService.ClearAsync(userId);
               return Ok(cart);
          }

          // ── POST /api/cart/{userId}/undo ─────────────────────────────────────
          /// <summary>
          /// Anuleaza ultima operatie executata pe cos (LIFO).
          /// Se poate apela succesiv pentru a anula mai multe operatii.
          /// Returneaza 409 daca nu exista operatii de anulat.
          /// </summary>
          [HttpPost("{userId:int}/undo")]
          public async Task<IActionResult> Undo(int userId)
          {
               var (success, message, cart) = await _cartService.UndoAsync(userId);

               if (!success)
                    return Conflict(new { Error = message, Cart = cart });

               return Ok(new { Message = message, Cart = cart });
          }

          // ── GET /api/cart/{userId}/history ───────────────────────────────────
          /// <summary>
          /// Returneaza jurnalul complet al operatiilor pe cos pentru user.
          /// Include si operatiile anulate (WasUndone = true).
          /// </summary>
          [HttpGet("{userId:int}/history")]
          public IActionResult GetHistory(int userId)
          {
               var history = _cartService.GetHistory(userId);
               return Ok(new
               {
                    UserId       = userId,
                    TotalEntries = history.Count,
                    History      = history
               });
          }

          // ── DELETE /api/cart/{userId}/session ────────────────────────────────
          /// <summary>
          /// Sterge cosul si tot istoricul comenzilor pentru user (ex: dupa checkout sau logout).
          /// </summary>
          [HttpDelete("{userId:int}/session")]
          public IActionResult ClearSession(int userId)
          {
               _cartService.DeleteCart(userId);
               return Ok(new { Message = $"Cosul si istoricul pentru user {userId} au fost sterse." });
          }

          // ════════════════════════════════════════════════════════════════════
          // COMENZI ORDER — Command Pattern (order operations)
          // ════════════════════════════════════════════════════════════════════

          // ── POST /api/cart/orders ────────────────────────────────────────────
          /// <summary>
          /// Plaseaza o comanda cu plata via PlaceOrderCommand.
          /// Undo in-scope: POST /api/cart/orders/undo (anuleaza comanda plasata).
          /// </summary>
          [HttpPost("orders")]
          public async Task<IActionResult> PlaceOrder([FromBody] OrderWithPaymentRequest request)
          {
               if (request == null || !request.Items.Any())
                    return BadRequest("Comanda trebuie sa contina cel putin un produs.");

               var result = await _orderCommandService.PlaceOrderAsync(request);

               return result.Success
                    ? Ok(result)
                    : BadRequest(result);
          }

          // ── POST /api/cart/orders/{id}/cancel ────────────────────────────────
          /// <summary>
          /// Anuleaza o comanda via CancelOrderCommand.
          /// Undo in-scope: POST /api/cart/orders/undo (restaureaza statusul anterior).
          /// </summary>
          [HttpPost("orders/{orderId:int}/cancel")]
          public async Task<IActionResult> CancelOrder(
               int orderId,
               [FromBody] OrderStatusRequest? notification = null)
          {
               var result = await _orderCommandService.CancelOrderAsync(orderId, notification);

               if (!result.Success)
                    return BadRequest(new { Error = result.Message });

               return Ok(result);
          }

          // ── PATCH /api/cart/orders/{id}/status ──────────────────────────────
          /// <summary>
          /// Actualizeaza statusul comenzii via UpdateOrderStatusCommand.
          /// Undo in-scope: POST /api/cart/orders/undo (revine la statusul anterior).
          /// </summary>
          [HttpPatch("orders/{orderId:int}/status")]
          public async Task<IActionResult> UpdateOrderStatus(
               int orderId,
               [FromBody] OrderStatusRequest request)
          {
               if (string.IsNullOrWhiteSpace(request?.NewStatus))
                    return BadRequest("Campul 'NewStatus' este obligatoriu.");

               var result = await _orderCommandService.UpdateStatusAsync(orderId, request);

               if (!result.Success)
                    return NotFound(new { Error = result.Message });

               return Ok(result);
          }

          // ── POST /api/cart/orders/undo ───────────────────────────────────────
          /// <summary>
          /// Anuleaza ultima operatie Order din sesiunea curenta (Scoped).
          /// PlaceOrder → anuleaza comanda
          /// CancelOrder → restaureaza statusul anterior
          /// UpdateStatus → revine la statusul anterior
          /// </summary>
          [HttpPost("orders/undo")]
          public async Task<IActionResult> UndoOrderOperation()
          {
               var result = await _orderCommandService.UndoLastAsync();

               if (result == null || !result.Success)
                    return Conflict(new { Error = result?.Message ?? "Nu exista operatii de anulat." });

               return Ok(result);
          }

          // ── GET /api/cart/orders/session-history ─────────────────────────────
          /// <summary>
          /// Returneaza istoricul comenzilor Order executate in aceasta sesiune HTTP.
          /// </summary>
          [HttpGet("orders/session-history")]
          public IActionResult GetOrderSessionHistory()
          {
               var history = _orderCommandService.GetSessionHistory();
               return Ok(new
               {
                    Count   = history.Count,
                    History = history
               });
          }
     }
}
