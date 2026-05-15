using CH_Store.Application.DBContext;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Events;

namespace CH_Store.Application.Order.Observer
{
     /// <summary>
     /// Observer Pattern — Observer #3: Audit log pentru dashboard admin.
     ///
     /// Responsabilitate: salveaza fiecare eveniment de schimbare a statusului
     /// in OrderEventLogDbTable (SQL Server, AppDbContext).
     ///
     /// Este intotdeauna activ — logheaza ORICE tranzitie de status,
     /// indiferent de canalul de notificare sau de existenta datelor de contact.
     /// Folosit de admin pentru vizualizarea istoricului complet al comenzilor.
     /// </summary>
     public class AdminDashboardObserver : IOrderObserver
     {
          private readonly AppDbContext _db;

          public string ObserverName => "AdminDashboardObserver";

          public AdminDashboardObserver(AppDbContext db)
          {
               _db = db;
          }

          public async Task OnOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent)
          {
               var logEntry = new OrderEventLogDbTable
               {
                    OrderId    = orderEvent.OrderId,
                    UserId     = orderEvent.UserId,
                    OldStatus  = orderEvent.OldStatus,
                    NewStatus  = orderEvent.NewStatus,
                    OrderTotal = orderEvent.OrderTotal,
                    ItemsCount = orderEvent.Items.Count,
                    OccurredAt = orderEvent.OccurredAt,
                    Notes      = BuildNotes(orderEvent)
               };

               _db.OrderEventLogs.Add(logEntry);
               await _db.SaveChangesAsync();

               Console.WriteLine($"[{ObserverName}] ✓ Log salvat: Comanda #{orderEvent.OrderId} " +
                                 $"{orderEvent.OldStatus} → {orderEvent.NewStatus} " +
                                 $"(Log ID: {logEntry.Id})");
          }

          private static string BuildNotes(OrderStatusChangedEvent e)
          {
               var parts = new List<string>();

               if (e.Items.Any())
                    parts.Add($"Produse: {string.Join(", ", e.Items.Select(i => $"{i.ProductName} x{i.Quantity}"))}");

               if (e.HasContactData)
                    parts.Add($"Notificat: {e.RecipientContact} ({e.NotificationChannel})");

               return parts.Any() ? string.Join(" | ", parts) : "";
          }
     }
}
