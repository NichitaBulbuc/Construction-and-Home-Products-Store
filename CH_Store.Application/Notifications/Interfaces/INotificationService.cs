using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Notifications.Interfaces
{
     public interface INotificationService
     {
          /// <summary>
          /// Trimite o notificare generica pe canalul specificat.
          /// Abstract Factory selecteaza familia corecta de Sender + Template.
          /// </summary>
          void Notify(NotificationData data, string channel, string recipient);

          // ─── Metode de convenienta pentru scenarii frecvente ─────────

          void NotifyOrderAccepted(int orderId, decimal total, string recipientName, string channel, string recipient);

          void NotifyOrderShipped(int orderId, DateTime estimatedDelivery, string recipientName, string channel, string recipient);

          void NotifyOrderDelivered(int orderId, string recipientName, string channel, string recipient);

          void NotifyOrderCancelled(int orderId, string recipientName, string channel, string recipient);

          void NotifyPromotion(string promoCode, string description, decimal discountPercent, string channel, string recipient);

          void NotifyWelcome(string recipientName, string channel, string recipient);

          IEnumerable<NotificationLog> GetLog();
     }
}
