using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Delivery
{
     /// <summary>
     /// Strategie: livrare in aceeasi zi — comanda plasata pana la 12:00, +250 MDL.
     /// </summary>
     public class SameDayDeliveryStrategy : IDeliveryStrategy
     {
          private const decimal Cost = 250m;

          public DeliveryType DeliveryType => DeliveryType.SameDay;

          public decimal CalculateCost(OrderData order) => Cost;

          public string GetDescription() => "Livrare Same Day — aceeasi zi (comanda pana la 12:00), +250 MDL";

          public int GetEstimatedDays() => 0;
     }
}
