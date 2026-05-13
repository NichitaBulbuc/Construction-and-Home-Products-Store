using CH_Store.Application.Order.Chain.Handlers;
using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Order.Chain
{
     /// <summary>
     /// Asambleaza si expune lantul complet de aprobare.
     ///
     /// Ordinea handlerelor este intentionata:
     ///   1. StockValidation   — verificare hard (Rejected opreste imediat)
     ///   2. CreditLimit       — verificare financiara B2B
     ///   3. DiscountApproval  — verificare comerciala
     ///   4. FraudDetection    — verificare de risc (ultima — are nevoie de date din context)
     ///
     /// StockValidation este singurul care poate Reject —
     /// restul pot doar PendingManagerApproval (nu blocheaza dar escaleaza).
     ///
     /// Inregistrat ca Scoped in DI deoarece handlerele au AppDbContext (Scoped).
     /// </summary>
     public class OrderApprovalChain : IOrderApprovalChain
     {
          private readonly IOrderApprovalHandler _head;

          public OrderApprovalChain(
               StockValidationHandler  stock,
               CreditLimitHandler      credit,
               DiscountApprovalHandler discount,
               FraudDetectionHandler   fraud)
          {
               // Inlantuite fluent: Stock → Credit → Discount → Fraud
               stock.SetNext(credit)
                    .SetNext(discount)
                    .SetNext(fraud);

               _head = stock;
          }

          /// <inheritdoc/>
          public Task<ApprovalResult> ProcessAsync(OrderApprovalContext context)
               => _head.HandleAsync(context);
     }
}
