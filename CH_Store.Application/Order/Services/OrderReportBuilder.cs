using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Strategy.Delivery;
using CH_Store.Application.Order.Strategy.Payment;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;
using System.Text;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Construieste un raport text detaliat al comenzii.
     /// Implementeaza acelasi IOrderBuilder ca OrderBuilder —
     /// Director-ul poate folosi aceeasi reteta pe ambii builderi.
     ///
     /// Strategy Pattern integrat:
     ///   • SetDelivery()         → DeliveryStrategyResolver furnizeaza descrierea si costul
     ///   • SetDeliveryStrategy() → primeste strategia direct
     ///   • SetPaymentStrategy()  → adauga linia de metoda de plata in raport
     ///
     /// Codul duplicat (switch cu costuri) a fost eliminat — un singur loc de adevar: strategia.
     /// </summary>
     public class OrderReportBuilder : IOrderBuilder
     {
          private StringBuilder _report = new();
          private decimal _itemsTotal;
          private decimal _deliveryCost;
          private bool    _hasInstallation;
          private bool    _isPriority;
          private decimal _discount;

          private const decimal InstallationCost = 500m;
          private const decimal PriorityCost     = 200m;

          public OrderReportBuilder() => Reset();

          public void Reset()
          {
               _report          = new StringBuilder();
               _itemsTotal      = 0;
               _deliveryCost    = 0;
               _hasInstallation = false;
               _isPriority      = false;
               _discount        = 0;

               _report.AppendLine("╔══════════════════════════════════════╗");
               _report.AppendLine("║          RAPORT COMANDA              ║");
               _report.AppendLine("╚══════════════════════════════════════╝");
               _report.AppendLine($"  Data: {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC");
               _report.AppendLine();
          }

          public void SetUserId(int userId)
               => _report.AppendLine($"  Client ID: {userId}");

          public void AddItem(int productId, string name, double price, int quantity)
          {
               decimal subtotal = (decimal)(price * quantity);
               _itemsTotal += subtotal;

               _report.AppendLine($"  Produs  : {name} (ID: {productId})");
               _report.AppendLine($"  Pret    : {price:F2} MDL  x  {quantity} buc  =  {subtotal:F2} MDL");
               _report.AppendLine();
          }

          /// <summary>
          /// Rezolva strategia prin DeliveryStrategyResolver si o aplica in raport.
          /// Inlocuieste switch-ul hardcodat cu costuri.
          /// </summary>
          public void SetDelivery(string address, DeliveryType deliveryType)
          {
               var strategy = DeliveryStrategyResolver.Resolve(deliveryType);
               ApplyDeliveryStrategy(address, strategy);
          }

          /// <summary>Primeste strategia direct si o aplica in raport.</summary>
          public void SetDeliveryStrategy(string address, IDeliveryStrategy strategy)
               => ApplyDeliveryStrategy(address, strategy);

          /// <summary>Adauga linia de metoda de plata in raport.</summary>
          public void SetPaymentStrategy(IPaymentStrategy strategy)
               => _report.AppendLine($"  Plata   : {strategy.GetDescription()}");

          public void AddInstallation()
          {
               _hasInstallation = true;
               _report.AppendLine($"  Montaj/Instalare inclus  (+{InstallationCost} MDL)");
          }

          public void EnablePriority()
          {
               _isPriority = true;
               _report.AppendLine($"  Procesare prioritara activata  (+{PriorityCost} MDL)");
          }

          public void SetDiscount(decimal discount)
          {
               _discount = discount;
               if (discount > 0)
                    _report.AppendLine($"  Reducere aplicata:  -{discount:F2} MDL");
          }

          public void SetNotes(string notes)
          {
               if (!string.IsNullOrWhiteSpace(notes))
               {
                    _report.AppendLine();
                    _report.AppendLine($"  Mentiuni: {notes}");
               }
          }

          /// <summary>Returneaza raportul text complet cu totalul calculat.</summary>
          public string GetResult()
          {
               decimal total = _itemsTotal + _deliveryCost;

               if (_hasInstallation) total += InstallationCost;
               if (_isPriority)      total += PriorityCost;

               total -= _discount;
               if (total < 0) total = 0;

               _report.AppendLine();
               _report.AppendLine("  ────────────────────────────────────");
               _report.AppendLine($"  Subtotal produse : {_itemsTotal,10:F2} MDL");
               _report.AppendLine($"  Livrare          : {_deliveryCost,10:F2} MDL");

               if (_hasInstallation)
                    _report.AppendLine($"  Montaj           : {InstallationCost,10:F2} MDL");

               if (_isPriority)
                    _report.AppendLine($"  Prioritate       : {PriorityCost,10:F2} MDL");

               if (_discount > 0)
                    _report.AppendLine($"  Reducere         : {-_discount,10:F2} MDL");

               _report.AppendLine("  ────────────────────────────────────");
               _report.AppendLine($"  TOTAL DE PLATA   : {total,10:F2} MDL");
               _report.AppendLine("  ════════════════════════════════════");

               return _report.ToString();
          }

          // ── Helper privat ────────────────────────────────────────────────────

          private void ApplyDeliveryStrategy(string address, IDeliveryStrategy strategy)
          {
               _deliveryCost = strategy.CalculateCost(new OrderData());

               string costLabel = _deliveryCost == 0
                    ? "gratuit"
                    : $"+{_deliveryCost} MDL";

               _report.AppendLine($"  Livrare : {strategy.GetDescription()}");
               _report.AppendLine($"  Termen  : ~{strategy.GetEstimatedDays()} zile lucratoare  ({costLabel})");
               _report.AppendLine($"  Adresa  : {address}");
               _report.AppendLine();
          }
     }
}
