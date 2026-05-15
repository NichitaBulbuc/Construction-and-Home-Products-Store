using CH_Store.Application.DBContext;
using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Notifications.Services
{
     /// <summary>
     /// Serviciul principal care orcheseaza Abstract Factory Pattern:
     ///
     ///   1. Primeste canalul dorit ("email" / "sms")
     ///   2. _factoryResolver selecteaza Concrete Factory-ul potrivit
     ///   3. Factory creeaza familia de produse: IMessageSender + ITemplateProvider
     ///   4. Template genereaza continutul pe baza NotificationData
     ///   5. Sender livreaza mesajul
     ///   6. Logheaza notificarea in NotificationContext
     /// </summary>
     public class NotificationService : INotificationService
     {
          private readonly Func<string, INotificationFactory> _factoryResolver;
          private readonly NotificationContext _context;

          public NotificationService(
               Func<string, INotificationFactory> factoryResolver,
               NotificationContext context)
          {
               _factoryResolver = factoryResolver;
               _context         = context;
          }

          // ─── Metoda centrala ─────────────────────────────────────────────
          public void Notify(NotificationData data, string channel, string recipient)
          {
               // Pasul 1 — Abstract Factory: selectam familia corecta
               INotificationFactory factory = _factoryResolver(channel.ToLower());

               // Pasul 2 — Cream produsele familiei
               IMessageSender   sender   = factory.CreateSender();
               ITemplateProvider template = factory.CreateTemplate();

               // Pasul 3 — Generam subiectul si continutul
               string subject = template.GetSubject(data);
               string content = template.GetContent(data);

               // Pasul 4 — Trimitem
               sender.Send(recipient, subject, content);

               // Pasul 5 — Logam in baza de date
               _context.NotificationLogs.Add(new NotificationLog
               {
                    Channel   = channel,
                    Recipient = recipient,
                    EventType = data.Event.ToString(),
                    Content   = content,
                    SentAt    = DateTime.UtcNow,
                    Success   = true
               });
               _context.SaveChanges();
          }

          // ─── Metode de convenienta ────────────────────────────────────────

          public void NotifyOrderAccepted(int orderId, decimal total, string recipientName, string channel, string recipient)
               => Notify(new NotificationData
               {
                    Event         = NotificationEvent.OrderAccepted,
                    RecipientName = recipientName,
                    OrderId       = orderId,
                    OrderTotal    = total
               }, channel, recipient);

          public void NotifyOrderShipped(int orderId, DateTime estimatedDelivery, string recipientName, string channel, string recipient)
               => Notify(new NotificationData
               {
                    Event             = NotificationEvent.OrderShipped,
                    RecipientName     = recipientName,
                    OrderId           = orderId,
                    EstimatedDelivery = estimatedDelivery
               }, channel, recipient);

          public void NotifyOrderDelivered(int orderId, string recipientName, string channel, string recipient)
               => Notify(new NotificationData
               {
                    Event         = NotificationEvent.OrderDelivered,
                    RecipientName = recipientName,
                    OrderId       = orderId
               }, channel, recipient);

          public void NotifyOrderCancelled(int orderId, string recipientName, string channel, string recipient)
               => Notify(new NotificationData
               {
                    Event         = NotificationEvent.OrderCancelled,
                    RecipientName = recipientName,
                    OrderId       = orderId
               }, channel, recipient);

          public void NotifyPromotion(string promoCode, string description, decimal discountPercent, string channel, string recipient)
               => Notify(new NotificationData
               {
                    Event           = NotificationEvent.PromotionAvailable,
                    RecipientName   = recipient,
                    PromoCode       = promoCode,
                    PromoDescription = description,
                    DiscountPercent = discountPercent
               }, channel, recipient);

          public void NotifyWelcome(string recipientName, string channel, string recipient)
               => Notify(new NotificationData
               {
                    Event         = NotificationEvent.Welcome,
                    RecipientName = recipientName
               }, channel, recipient);

          public IEnumerable<NotificationLog> GetLog()
               => _context.NotificationLogs.OrderByDescending(l => l.SentAt).ToList();
     }
}
