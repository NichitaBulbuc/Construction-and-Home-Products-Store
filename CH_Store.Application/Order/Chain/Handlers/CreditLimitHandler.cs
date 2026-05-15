using CH_Store.Application.Order.Chain;
using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Order.Chain.Handlers
{
     /// <summary>
     /// Handler 2/4 — Limita de credit B2B.
     ///
     /// Pentru comenzi care depasesc pragul B2B (50 000 MDL implicit),
     /// comanda nu este respinsa, dar necesita aprobare din partea
     /// unui manager financiar inainte de procesare.
     ///
     /// Decizie:
     ///   • Total ≤ B2BThreshold              → Approved
     ///   • Total > B2BThreshold              → PendingManagerApproval
     ///
     /// Motivatie: comenzile mari pot depasi creditul disponibil al clientului
     /// sau necesita verificare manuala a solvabilitatii.
     /// </summary>
     public class CreditLimitHandler : OrderApprovalHandlerBase
     {
          /// <summary>Pragul peste care se cere aprobare manager financiar (MDL).</summary>
          private const decimal B2BThreshold = 50_000m;

          public override string HandlerName => "CreditLimit";

          protected override Task<ApprovalCheck> ValidateAsync(OrderApprovalContext context)
          {
               if (context.ComputedTotal <= B2BThreshold)
                    return Task.FromResult(Approve(
                         $"Total {context.ComputedTotal:F2} MDL — sub limita B2B " +
                         $"({B2BThreshold:N0} MDL). Fara restrictii de credit."));

               return Task.FromResult(Pending(
                    $"Comanda de {context.ComputedTotal:F2} MDL depaseste limita de credit " +
                    $"B2B ({B2BThreshold:N0} MDL). " +
                    $"Necesita aprobare manager financiar inainte de procesare."));
          }
     }
}
