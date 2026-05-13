using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.Enums;
using System.Text;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Construieste un raport text detaliat al comenzii.
     /// Implementeaza acelasi IOrderBuilder ca OrderBuilder —
     /// Director-ul poate folosi aceeasi reteta pe ambii builderi.
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

          public void SetDelivery(string address, DeliveryType deliveryType)
          {
               _deliveryCost = deliveryType switch
               {
                    DeliveryType.Express => 100m,
                    DeliveryType.SameDay => 250m,
                    _                   => 0m
               };

               _report.AppendLine($"  Livrare : {deliveryType}  ({(_deliveryCost == 0 ? "gratuit" : $"+{_deliveryCost} MDL")})");
               _report.AppendLine($"  Adresa  : {address}");
               _report.AppendLine();
          }

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
     }
}
