using CH_Store.Application.DBContext;
using CH_Store.Domain.Events;

namespace CH_Store.Application.Order.Observer
{
     /// <summary>
     /// Observer Pattern — Observer #2: Actualizare stocuri.
     ///
     /// Responsabilitate: sincronizeaza ProductDbTable.StockQuantity din AppDbContext
     /// (SQL Server) cand statusul comenzii se schimba.
     ///
     /// Logica de stoc:
     ///   "Processing"   → DECREMENTEAZA stocul (comanda confirmata, produsele rezervate)
     ///   "Cancelled"    → RESTAUREAZA stocul (comanda anulata inainte de expediere)
     ///   "PaymentFailed"→ RESTAUREAZA stocul (plata esuata, produsele eliberate)
     ///   "Shipped"      → fara actiune (stocul deja decrementat la Processing)
     ///   "Delivered"    → fara actiune (stocul deja decrementat la Processing)
     ///   Alte statusuri → fara actiune
     ///
     /// Daca un produs nu mai exista in DB (sters ulterior), avertisment in log,
     /// continuare pentru celelalte produse.
     /// Stocul nu scade sub 0 la decrementare (min = 0).
     /// </summary>
     public class StockObserver : IOrderObserver
     {
          private readonly AppDbContext _db;

          public string ObserverName => "StockObserver";

          public StockObserver(AppDbContext db)
          {
               _db = db;
          }

          public async Task OnOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent)
          {
               string status = orderEvent.NewStatus.ToLower();

               if (status == "processing")
               {
                    await AdjustStockAsync(orderEvent, decrement: true);
               }
               else if (status is "cancelled" or "paymentfailed")
               {
                    await AdjustStockAsync(orderEvent, decrement: false);
               }
               else
               {
                    Console.WriteLine($"[{ObserverName}] Status '{orderEvent.NewStatus}' — fara modificare stoc.");
               }
          }

          // ─── Privat ──────────────────────────────────────────────────────────

          private async Task AdjustStockAsync(OrderStatusChangedEvent orderEvent, bool decrement)
          {
               string action = decrement ? "DECREMENTARE" : "RESTAURARE";
               Console.WriteLine($"[{ObserverName}] {action} stoc pentru {orderEvent.Items.Count} produs(e)...");

               foreach (var item in orderEvent.Items)
               {
                    var product = await _db.Products.FindAsync(item.ProductId);

                    if (product == null)
                    {
                         Console.WriteLine($"[{ObserverName}] ⚠ Produsul ID={item.ProductId} ({item.ProductName}) " +
                                           $"nu mai exista in DB — sarit.");
                         continue;
                    }

                    int stockBefore = product.StockQuantity;

                    if (decrement)
                         product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                    else
                         product.StockQuantity += item.Quantity;

                    Console.WriteLine($"[{ObserverName}] ✓ {product.Name}: " +
                                      $"stoc {stockBefore} → {product.StockQuantity} " +
                                      $"({(decrement ? "-" : "+")}{item.Quantity})");
               }

               await _db.SaveChangesAsync();
               Console.WriteLine($"[{ObserverName}] Stocuri salvate in SQL Server.");
          }
     }
}
