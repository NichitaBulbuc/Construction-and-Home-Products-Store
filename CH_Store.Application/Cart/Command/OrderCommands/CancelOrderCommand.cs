using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Cart.Command.OrderCommands
{
     /// <summary>
     /// Concrete Order Command — anuleaza o comanda existenta.
     ///
     /// Execute: seteaza status "Cancelled"
     /// Undo:    restaureaza statusul anterior (poate fi reversibil doar daca
     ///          procesarea nu a continuat intre timp)
     /// </summary>
     public class CancelOrderCommand : IOrderCommand
     {
          private readonly int                _orderId;
          private readonly OrderStatusRequest _notification;

          // Snapshot pentru Undo
          private string _statusBeforeCancel = "";

          public CancelOrderCommand(int orderId, OrderStatusRequest? notification = null)
          {
               _orderId      = orderId;
               _notification = notification ?? new OrderStatusRequest();
          }

          public string   CommandName => nameof(CancelOrderCommand);
          public string   Description => $"Anulata comanda #{_orderId}";
          public DateTime OccurredAt  { get; } = DateTime.UtcNow;
          public bool     CanUndo     => !string.IsNullOrWhiteSpace(_statusBeforeCancel);

          public async Task<OrderOperationResult> ExecuteAsync(IOrderFacade facade)
          {
               // Salveaza statusul curent inainte de anulare (necesar pentru Undo)
               var order = await facade.GetOrderAsync(_orderId);
               if (order != null)
                    _statusBeforeCancel = order.Status;

               return await facade.CancelOrderAsync(_orderId, _notification);
          }

          /// <summary>
          /// Undo = restaureaza statusul anterior anularii.
          /// Util daca utilizatorul a anulat accidental.
          /// </summary>
          public async Task<OrderOperationResult> UndoAsync(IOrderFacade facade)
          {
               if (string.IsNullOrWhiteSpace(_statusBeforeCancel))
                    return new OrderOperationResult
                    {
                         Success  = false,
                         OrderId  = _orderId,
                         Message  = "Nu exista snapshot de status — Undo imposibil."
                    };

               return await facade.UpdateOrderStatusAsync(_orderId, new OrderStatusRequest
               {
                    NewStatus = _statusBeforeCancel,
                    Reason    = $"Restaurat prin Command Undo (status anterior anularii: {_statusBeforeCancel})."
               });
          }
     }
}
