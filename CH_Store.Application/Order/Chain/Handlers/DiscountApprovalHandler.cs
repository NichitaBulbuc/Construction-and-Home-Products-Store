using CH_Store.Application.Order.Chain;
using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Order.Chain.Handlers
{
     /// <summary>
     /// Handler 3/4 — Aprobare discount mare.
     ///
     /// Un discount care depaseste 20% din valoarea produselor
     /// nu poate fi aprobat automat — necesita acordul managerului de vanzari.
     ///
     /// Decizie:
     ///   • Discount = 0 sau subtotal = 0 → Approved (nimic de verificat)
     ///   • Discount% ≤ MaxAutoDiscount    → Approved
     ///   • Discount% > MaxAutoDiscount    → PendingManagerApproval
     ///
     /// Imbogatire context:
     ///   Seteaza context.DiscountPercent pentru vizibilitate in raspunsul API.
     /// </summary>
     public class DiscountApprovalHandler : OrderApprovalHandlerBase
     {
          /// <summary>Procentul maxim aprobat automat fara interventie manager.</summary>
          private const double MaxAutoDiscountPercent = 20.0;

          public override string HandlerName => "DiscountApproval";

          protected override Task<ApprovalCheck> ValidateAsync(OrderApprovalContext context)
          {
               if (context.Request.Discount <= 0 || context.ComputedSubtotal <= 0)
                    return Task.FromResult(Approve("Fara discount aplicat sau discount in limite normale."));

               double pct = (double)(context.Request.Discount / context.ComputedSubtotal) * 100.0;
               context.DiscountPercent = pct;

               if (pct <= MaxAutoDiscountPercent)
                    return Task.FromResult(Approve(
                         $"Discount {pct:F1}% ({context.Request.Discount:F2} MDL) — " +
                         $"in limitele de aprobare automata (≤{MaxAutoDiscountPercent}%)."));

               return Task.FromResult(Pending(
                    $"Discount {pct:F1}% ({context.Request.Discount:F2} MDL din " +
                    $"{context.ComputedSubtotal:F2} MDL) depaseste pragul de {MaxAutoDiscountPercent}%. " +
                    $"Necesita aprobare manager de vanzari."));
          }
     }
}
