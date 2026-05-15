using CH_Store.Domain.DTOs;
using CH_Store.Domain.Enums;

namespace CH_Store.Application.Order.Chain
{
     /// <summary>
     /// Clasa de baza abstracta pentru handlere.
     ///
     /// Implementeaza logica de inlantuire si agregare:
     ///   • Rejected     → opreste lantul imediat
     ///   • Pending      → continua lantul, semnalul ramane in context.Checks
     ///   • Approved     → continua lantul
     ///   • Sfarsit lant → agrega toate Checks-urile, calculeaza statusul final
     ///
     /// Handlerele concrete implementeaza doar ValidateAsync() —
     /// nu trebuie sa stie nimic despre vecinii din lant.
     /// </summary>
     public abstract class OrderApprovalHandlerBase : IOrderApprovalHandler
     {
          private IOrderApprovalHandler? _next;

          public abstract string HandlerName { get; }

          public IOrderApprovalHandler SetNext(IOrderApprovalHandler next)
          {
               _next = next;
               return next;   // fluent: h1.SetNext(h2).SetNext(h3)
          }

          public async Task<ApprovalResult> HandleAsync(OrderApprovalContext context)
          {
               // Executa validarea acestui handler
               var check = await ValidateAsync(context);
               context.Checks.Add(check);

               // Rejected: opreste lantul imediat — nu mai are rost sa continuam
               if (check.Status == ApprovalStatus.Rejected)
                    return ApprovalResult.Reject(
                         new List<ApprovalCheck>(context.Checks),
                         check.Message);

               // Continua spre urmatorul handler (daca exista)
               if (_next != null)
                    return await _next.HandleAsync(context);

               // ─── Sfarsit de lant — agrega rezultatul final ───────────────────
               bool anyPending = context.Checks
                    .Any(c => c.Status == ApprovalStatus.PendingManagerApproval);

               if (anyPending)
               {
                    var reasons = context.Checks
                         .Where(c => c.Status == ApprovalStatus.PendingManagerApproval)
                         .Select(c => c.Message)
                         .ToList();

                    return ApprovalResult.Pending(
                         new List<ApprovalCheck>(context.Checks),
                         reasons);
               }

               return ApprovalResult.Approve(new List<ApprovalCheck>(context.Checks));
          }

          // ── Subclasele implementeaza DOAR aceasta metoda ──────────────────────

          /// <summary>
          /// Returneaza decizia acestui handler.
          /// Foloseste metodele helper Approve / Reject / Pending pentru lizibilitate.
          /// </summary>
          protected abstract Task<ApprovalCheck> ValidateAsync(OrderApprovalContext context);

          // ── Helper factory methods ─────────────────────────────────────────────

          protected ApprovalCheck Approve(string message) => new()
          {
               HandlerName = HandlerName,
               Status      = ApprovalStatus.Approved,
               Message     = message
          };

          protected ApprovalCheck Reject(string message) => new()
          {
               HandlerName = HandlerName,
               Status      = ApprovalStatus.Rejected,
               Message     = message
          };

          protected ApprovalCheck Pending(string message) => new()
          {
               HandlerName = HandlerName,
               Status      = ApprovalStatus.PendingManagerApproval,
               Message     = message
          };
     }
}
