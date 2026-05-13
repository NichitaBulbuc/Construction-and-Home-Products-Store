using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Delivery
{
     /// <summary>Strategie: ridicare din depozit — fara cost, disponibil in 24h.</summary>
     public class PickupDeliveryStrategy : IDeliveryStrategy
     {
          public DeliveryType DeliveryType => DeliveryType.Pickup;

          public decimal CalculateCost(OrderData order) => 0m;

          public string GetDescription() => "Ridicare din depozit — gratuit, disponibil in 24h";

          public int GetEstimatedDays() => 1;
     }
}
