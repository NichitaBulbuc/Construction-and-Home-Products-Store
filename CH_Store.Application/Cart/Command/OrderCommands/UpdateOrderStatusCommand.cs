using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Cart.Command.OrderCommands
{
     /// <summary>
     /// Concrete Order Command — schimba statusul unei comenzi.
     ///
     /// Execute: seteaza NewStatus
     /// Undo:    revine la OldStatus (snapshot capturat la Execute)
     ///
     /// Exemple de flux:
     ///   New → Processing    (undo → New)
     ///   Processing → Shipped (undo → Processing)
     ///   Shipped → Delivered  (undo → Shipped) — administrativ
     /// </summary>
     public class UpdateOrderStatusCommand : IOrderCommand
     {
          private readonly int                _orderId;
          private readonly OrderStatusRequest _request;

          // Snapshot pentru Undo
          private string _statusBeforeUpdate = "";

          public UpdateOrderStatusCommand(int orderId, OrderStatusRequest request)
          {
               _orderId = orderId;
               _request = request;
          }

          public string   CommandName => nameof(UpdateOrderStatusCommand);
          public string   Description => $"Status comanda #{_orderId}: → {_request.NewStatus}";
          public DateTime OccurredAt  { get; } = DateTime.UtcNow;
          public bool     CanUndo     => !string.IsNullOrWhiteSpace(_statusBeforeUpdate);

          public async Task<OrderOperationResult> ExecuteAsync(IOrderFacade facade)
          {
               // Captureaza statusul curent inainte de modificare
               var order = await facade.GetOrderAsync(_orderId);
               if (order != null)
                    _statusBeforeUpdate = order.Status;

               return await facade.UpdateOrderStatusAsync(_orderId, _request);
          }

          public async Task<OrderOperationResult> UndoAsync(IOrderFacade facade)
          {
               if (string.IsNullOrWhiteSpace(_statusBeforeUpdate))
                    return new OrderOperationResult
                    {
                         Success = false,
                         OrderId = _orderId,
                         Message = "Nu exista snapshot de status — Undo imposibil."
                    };

               return await facade.UpdateOrderStatusAsync(_orderId, new OrderStatusRequest
               {
                    NewStatus = _statusBeforeUpdate,
                    Reason    = $"Revert prin Command Undo (de la {_request.NewStatus} la {_statusBeforeUpdate})."
               });
          }
     }
}
