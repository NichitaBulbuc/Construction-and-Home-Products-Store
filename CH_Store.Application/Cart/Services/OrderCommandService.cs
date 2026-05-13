using CH_Store.Application.Cart.Command.OrderCommands;
using CH_Store.Application.Cart.Interfaces;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Cart.Services
{
     /// <summary>
     /// Orchestrator pentru comenzile de Order via Command Pattern.
     ///
     /// Scoped — traieste per HTTP request.
     /// Stiva de undo este in-memory (Stack local), valabila in scope-ul curent.
     ///
     /// De ce Scoped si nu Singleton?
     ///   IOrderFacade este Scoped (depinde de DbContext). Comenzile Order
     ///   primesc IOrderFacade la Execute/Undo — nu il stocheaza —
     ///   deci nu exista problema de lifetime. OrderCommandService
     ///   poate fi Scoped fara grija.
     ///
     /// De ce stiva locala (nu IMemoryCache)?
     ///   Comenzile Order implica operatii DB (PlaceOrder, Cancel) care nu pot
     ///   fi "suspendate" intre request-uri. Undo-ul are sens in acelasi context
     ///   tranzactional. Daca utilizatorul vrea sa anuleze o comanda plasata
     ///   anterior, foloseste direct CancelOrder (nu Undo inter-request).
     /// </summary>
     public class OrderCommandService : IOrderCommandService
     {
          private readonly IOrderFacade         _facade;
          private readonly Stack<IOrderCommand> _history = new();

          public OrderCommandService(IOrderFacade facade)
               => _facade = facade;

          // ── Execute ──────────────────────────────────────────────────────────

          public async Task<OrderOperationResult> PlaceOrderAsync(OrderWithPaymentRequest request)
          {
               var command = new PlaceOrderCommand(request);
               return await ExecuteInternalAsync(command);
          }

          public async Task<OrderOperationResult> CancelOrderAsync(int orderId, OrderStatusRequest? notification = null)
          {
               var command = new CancelOrderCommand(orderId, notification);
               return await ExecuteInternalAsync(command);
          }

          public async Task<OrderOperationResult> UpdateStatusAsync(int orderId, OrderStatusRequest request)
          {
               var command = new UpdateOrderStatusCommand(orderId, request);
               return await ExecuteInternalAsync(command);
          }

          // ── Undo ─────────────────────────────────────────────────────────────

          public async Task<OrderOperationResult?> UndoLastAsync()
          {
               if (_history.Count == 0)
                    return new OrderOperationResult
                    {
                         Success = false,
                         Message = "Nu exista operatii de anulat in aceasta sesiune."
                    };

               var command = _history.Pop();

               if (!command.CanUndo)
                    return new OrderOperationResult
                    {
                         Success = false,
                         Message = $"Comanda '{command.CommandName}' nu are Undo disponibil (a esuat la Execute)."
                    };

               return await command.UndoAsync(_facade);
          }

          // ── Interogari ───────────────────────────────────────────────────────

          public IReadOnlyList<string> GetSessionHistory()
               => _history
                    .Reverse()   // cronologic: primul executat primul in lista
                    .Select(c => $"[{c.OccurredAt:HH:mm:ss}] {c.Description}")
                    .ToList();

          // ── Helper ───────────────────────────────────────────────────────────

          private async Task<OrderOperationResult> ExecuteInternalAsync(IOrderCommand command)
          {
               var result = await command.ExecuteAsync(_facade);
               if (result.Success)
                    _history.Push(command);
               return result;
          }
     }
}
