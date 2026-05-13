using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Order.Chain
{
     /// <summary>
     /// Chain of Responsibility — interfata handler.
     ///
     /// Fiecare handler:
     ///   1. Executa propria verificare pe context
     ///   2. Daca respinge → opreste lantul, returneaza ApprovalResult.Rejected
     ///   3. Daca aproba  → paseaza catre urmatorul handler (SetNext)
     ///   4. Daca marcheaza ca pending → continua lantul, dar semnalul ramane in context
     /// </summary>
     public interface IOrderApprovalHandler
     {
          string HandlerName { get; }

          /// <summary>
          /// Leaga urmatorul handler in lant.
          /// Returneaza <paramref name="next"/> pentru inlantuire fluenta:
          ///   h1.SetNext(h2).SetNext(h3).SetNext(h4)
          /// </summary>
          IOrderApprovalHandler SetNext(IOrderApprovalHandler next);

          Task<ApprovalResult> HandleAsync(OrderApprovalContext context);
     }
}
