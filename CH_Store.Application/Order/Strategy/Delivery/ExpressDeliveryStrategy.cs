using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Delivery
{
     /// <summary>Strategie: livrare expres — 1-2 zile lucratoare, +100 MDL.</summary>
     public class ExpressDeliveryStrategy : IDeliveryStrategy
     {
          private const decimal Cost = 100m;

          public DeliveryType DeliveryType => DeliveryType.Express;

          public decimal CalculateCost(OrderData order) => Cost;

          public string GetDescription() => "Livrare Express — 1-2 zile lucratoare, +100 MDL";

          public int GetEstimatedDays() => 2;
     }
}
