using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Cart.Interfaces
{
     /// <summary>
     /// Serviciu pentru operatii pe comenzi via Command Pattern.
     /// Pastreaza un istoric in-scope al comenzilor executate (Undo disponibil in aceeasi sesiune).
     /// </summary>
     public interface IOrderCommandService
     {
          Task<OrderOperationResult> PlaceOrderAsync(OrderWithPaymentRequest request);
          Task<OrderOperationResult> CancelOrderAsync(int orderId, OrderStatusRequest? notification = null);
          Task<OrderOperationResult> UpdateStatusAsync(int orderId, OrderStatusRequest request);

          /// <summary>
          /// Anuleaza ultima operatie executata in aceasta sesiune.
          /// (Disponibil in scope-ul HTTP request curent sau via injectie Scoped.)
          /// </summary>
          Task<OrderOperationResult?> UndoLastAsync();

          /// <summary>Istoricul comenzilor executate in aceasta sesiune.</summary>
          IReadOnlyList<string> GetSessionHistory();
     }
}
