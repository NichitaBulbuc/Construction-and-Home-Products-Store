using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Cart.Command.OrderCommands
{
     /// <summary>
     /// Concrete Order Command — plaseaza o comanda completa cu plata.
     ///
     /// Execute: apeleaza IOrderFacade.PlaceOrderWithPaymentAsync
     /// Undo:    anuleaza comanda plasata (IOrderFacade.CancelOrderAsync)
     ///          — posibil doar daca statusul nu este inca "Delivered"
     /// </summary>
     public class PlaceOrderCommand : IOrderCommand
     {
          private readonly OrderWithPaymentRequest _request;

          // Setat dupa Execute — necesar pentru Undo
          private int _placedOrderId;

          public PlaceOrderCommand(OrderWithPaymentRequest request)
               => _request = request;

          public string   CommandName => nameof(PlaceOrderCommand);
          public string   Description => $"Plasata comanda pentru user {_request.UserId} " +
                                         $"({_request.Items.Count} produse, metoda: {_request.PaymentMethod})";
          public DateTime OccurredAt  { get; } = DateTime.UtcNow;
          public bool     CanUndo     => _placedOrderId > 0;

          public async Task<OrderOperationResult> ExecuteAsync(IOrderFacade facade)
          {
               var response = await facade.PlaceOrderWithPaymentAsync(_request);

               _placedOrderId = response.DbId;

               return new OrderOperationResult
               {
                    Success   = true,
                    OrderId   = response.DbId,
                    NewStatus = response.Status,
                    Message   = $"Comanda #{response.DbId} plasata. " +
                                $"Plata: {(response.PaymentSuccess ? "reusita" : "esuata")}. " +
                                $"Status: {response.Status}."
               };
          }

          /// <summary>
          /// Undo = anulare comanda.
          /// Disponibil daca comanda nu a ajuns in status "Delivered".
          /// </summary>
          public async Task<OrderOperationResult> UndoAsync(IOrderFacade facade)
          {
               if (_placedOrderId == 0)
                    return new OrderOperationResult
                    {
                         Success = false,
                         Message = "Comanda nu a fost plasata — nu exista ce anula."
                    };

               return await facade.CancelOrderAsync(_placedOrderId, new OrderStatusRequest
               {
                    Reason = "Anulata prin Command Undo (utilizatorul a retras comanda)."
               });
          }
     }
}
