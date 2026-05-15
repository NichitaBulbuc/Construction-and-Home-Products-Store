using CH_Store.Domain.Enums;

namespace CH_Store.Application.Order.Strategy.Delivery
{
     /// <summary>
     /// Resolver (registry) pentru strategiile de livrare.
     ///
     /// Inlocuieste dictionarul static <DeliveryType, decimal> _deliveryCosts din OrderBuilder.
     /// Strategiile sunt Singleton-uri pure (fara stare) — se instantiaza o singura data.
     ///
     /// Utilizare:
     ///   IDeliveryStrategy strategy = DeliveryStrategyResolver.Resolve(DeliveryType.Express);
     ///   decimal cost = strategy.CalculateCost(order);
     /// </summary>
     public static class DeliveryStrategyResolver
     {
          private static readonly Dictionary<DeliveryType, IDeliveryStrategy> _strategies = new()
          {
               { DeliveryType.Standard, new StandardDeliveryStrategy() },
               { DeliveryType.Express,  new ExpressDeliveryStrategy()  },
               { DeliveryType.SameDay,  new SameDayDeliveryStrategy()  },
               { DeliveryType.Pickup,   new PickupDeliveryStrategy()   }
          };

          /// <summary>
          /// Returneaza strategia corespunzatoare tipului de livrare.
          /// </summary>
          /// <exception cref="ArgumentOutOfRangeException">Daca tipul nu are strategie inregistrata.</exception>
          public static IDeliveryStrategy Resolve(DeliveryType type)
          {
               return _strategies.TryGetValue(type, out var strategy)
                    ? strategy
                    : throw new ArgumentOutOfRangeException(
                         nameof(type),
                         $"Nicio strategie de livrare inregistrata pentru tipul '{type}'.");
          }

          /// <summary>Returneaza toate strategiile disponibile (pentru endpoint-ul /strategies).</summary>
          public static IReadOnlyDictionary<DeliveryType, IDeliveryStrategy> GetAll() => _strategies;
     }
}
