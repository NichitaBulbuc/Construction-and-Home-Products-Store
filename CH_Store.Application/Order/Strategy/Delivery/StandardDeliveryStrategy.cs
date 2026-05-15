using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Delivery
{
     /// <summary>Strategie: livrare standard — 3-5 zile lucratoare, fara cost suplimentar.</summary>
     public class StandardDeliveryStrategy : IDeliveryStrategy
     {
          public DeliveryType DeliveryType => DeliveryType.Standard;

          public decimal CalculateCost(OrderData order) => 0m;

          public string GetDescription() => "Livrare Standard — 3-5 zile lucratoare, gratuit";

          public int GetEstimatedDays() => 4;
     }
}
