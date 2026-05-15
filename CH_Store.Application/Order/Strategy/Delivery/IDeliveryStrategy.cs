using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Delivery
{
     /// <summary>
     /// Strategy Pattern — interfata comuna pentru toate strategiile de livrare.
     ///
     /// Inlocuieste dictionarul hardcodat <DeliveryType, decimal> din OrderBuilder.
     /// Fiecare strategie encapsuleaza algoritmul de calcul al costului,
     /// descrierea si termenul estimat de livrare.
     ///
     /// Beneficii:
     ///   • Adaugarea unui nou tip de livrare = o noua clasa, fara if-uri
     ///   • Strategia poate fi schimbata la runtime (ex: promotii, zone geografice)
     ///   • OrderBuilder nu mai stie de costuri — le delega strategiei
     /// </summary>
     public interface IDeliveryStrategy
     {
          /// <summary>Tipul de livrare reprezentat de aceasta strategie.</summary>
          DeliveryType DeliveryType { get; }

          /// <summary>Calculeaza costul de livrare pentru comanda data.</summary>
          decimal CalculateCost(OrderData order);

          /// <summary>Descriere lizibila afisata in raport si raspuns API.</summary>
          string GetDescription();

          /// <summary>Numarul estimat de zile lucratoare pana la livrare.</summary>
          int GetEstimatedDays();
     }
}
