using CH_Store.Domain.Entities;
using CH_Store.Domain.Models;
using System.Text;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Genereaza devizul estimativ text cu comparatia preturilor vechi → noi.
     /// Clasa statica — nu are dependente externe, primeste datele deja calculate.
     /// </summary>
     public static class DevizGenerator
     {
          public static string Generate(
               OrderDbTable  template,
               OrderData     clone,
               List<ItemPriceUpdate> priceUpdates)
          {
               var sb = new StringBuilder();

               double oldProductsTotal = priceUpdates.Sum(i => i.OldSubtotal);
               double newProductsTotal = priceUpdates.Sum(i => i.NewSubtotal);
               double oldGrandTotal    = (double)template.TotalAmount;
               double newGrandTotal    = (double)clone.TotalPrice;
               double totalDiff        = newGrandTotal - oldGrandTotal;
               double diffPercent      = oldGrandTotal > 0
                    ? totalDiff / oldGrandTotal * 100
                    : 0;

               var changedItems = priceUpdates.Where(p => p.PriceChanged).ToList();

               // ── Header ──────────────────────────────────────────────────
               sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
               sb.AppendLine("║                    DEVIZ ESTIMATIV                       ║");
               sb.AppendLine("║                      CH STORE                            ║");
               sb.AppendLine("╚══════════════════════════════════════════════════════════╝");
               sb.AppendLine($"  Data deviz   : {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC");
               sb.AppendLine($"  Clonat din   : Comanda #{template.Id}  ({template.OrderDate:dd.MM.yyyy})");
               sb.AppendLine($"  Client ID    : {clone.UserId}");
               sb.AppendLine($"  Adresa       : {clone.DeliveryAddress}");
               sb.AppendLine($"  Livrare      : {clone.DeliveryType}" +
                             $"  ({(clone.DeliveryCost == 0 ? "gratuit" : $"+{clone.DeliveryCost:F2} MDL")})");

               if (clone.HasInstallation) sb.AppendLine("  Montaj       : inclus (+500.00 MDL)");
               if (clone.IsPriority)      sb.AppendLine("  Prioritate   : activa (+200.00 MDL)");
               if (clone.Discount > 0)    sb.AppendLine($"  Reducere     : -{clone.Discount:F2} MDL");
               if (!string.IsNullOrWhiteSpace(clone.Notes))
                    sb.AppendLine($"  Mentiuni     : {clone.Notes}");

               // ── Produse ─────────────────────────────────────────────────
               sb.AppendLine();
               sb.AppendLine("  ── PRODUSE ──────────────────────────────────────────────");

               int idx = 1;
               foreach (var item in priceUpdates)
               {
                    sb.AppendLine();
                    sb.Append($"  {idx++,2}. {item.ProductName}");

                    if (item.PriceNotFound)
                         sb.AppendLine(" [PRODUS NEGASIT IN DB — pret mentinut din template]");
                    else
                         sb.AppendLine();

                    sb.AppendLine($"      Cantitate  : {item.Quantity} buc");

                    if (item.PriceChanged)
                    {
                         string dir = item.UnitDifference > 0 ? "▲ scumpire" : "▼ ieftinire";
                         sb.AppendLine($"      Pret/buc   : {item.OldPrice,8:F2} MDL  →  {item.NewPrice,8:F2} MDL" +
                                       $"  [{dir}: {(item.UnitDifference > 0 ? "+" : "")}{item.UnitDifference:F2} MDL]");
                    }
                    else
                    {
                         sb.AppendLine($"      Pret/buc   : {item.NewPrice,8:F2} MDL  (neschimbat)");
                    }

                    sb.AppendLine($"      Subtotal   : {item.OldSubtotal,8:F2}  →  {item.NewSubtotal,8:F2} MDL");

                    if (item.PriceChanged)
                    {
                         string sign = item.LineDifference >= 0 ? "+" : "";
                         sb.AppendLine($"      Δ linie    : {sign}{item.LineDifference:F2} MDL");
                    }
               }

               // ── Totaluri ────────────────────────────────────────────────
               sb.AppendLine();
               sb.AppendLine("  ────────────────────────────────────────────────────────");
               sb.AppendLine($"  Subtotal produse (vechi) : {oldProductsTotal,10:F2} MDL");
               sb.AppendLine($"  Subtotal produse (nou)   : {newProductsTotal,10:F2} MDL");
               sb.AppendLine();
               sb.AppendLine($"  TOTAL TEMPLATE           : {oldGrandTotal,10:F2} MDL");
               sb.AppendLine($"  TOTAL DEVIZ NOU          : {newGrandTotal,10:F2} MDL");
               sb.AppendLine($"  ────────────────────────────────────────────────────────");

               string diffSign = totalDiff >= 0 ? "+" : "";
               sb.AppendLine($"  DIFERENTA                : {diffSign}{totalDiff,9:F2} MDL" +
                             $"  ({diffSign}{diffPercent:F1}%)");

               sb.AppendLine("  ════════════════════════════════════════════════════════");

               // ── Sumar preturi modificate ─────────────────────────────────
               if (changedItems.Any())
               {
                    sb.AppendLine();
                    sb.AppendLine($"  ⚠  {changedItems.Count} produs(e) cu pret actualizat:");
                    foreach (var c in changedItems)
                    {
                         string sign = c.UnitDifference >= 0 ? "+" : "";
                         sb.AppendLine($"     • {c.ProductName,-30}" +
                                       $"  {c.OldPrice:F2} → {c.NewPrice:F2} MDL" +
                                       $"  ({sign}{c.UnitDifference:F2}/buc)");
                    }
               }
               else
               {
                    sb.AppendLine();
                    sb.AppendLine("  ✓  Toate preturile sunt identice cu template-ul.");
               }

               return sb.ToString();
          }
     }
}
