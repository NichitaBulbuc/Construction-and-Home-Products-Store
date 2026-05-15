using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Order.Chain
{
     /// <summary>
     /// Interfata lantului complet de aprobare a comenzii.
     /// Injectat in OrderFacade — ascunde constructia lantului.
     /// </summary>
     public interface IOrderApprovalChain
     {
          /// <summary>
          /// Ruleaza contextul prin toti cei 4 handleri in ordine:
          /// StockValidation → CreditLimit → DiscountApproval → FraudDetection
          /// </summary>
          Task<ApprovalResult> ProcessAsync(OrderApprovalContext context);
     }
}
