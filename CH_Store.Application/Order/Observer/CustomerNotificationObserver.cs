using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Domain.Events;

namespace CH_Store.Application.Order.Observer
{
     /// <summary>
     /// Observer Pattern — Observer #1: Notificare client.
     ///
     /// Responsabilitate: trimite email sau SMS clientului cand statusul comenzii se schimba.
     /// Foloseste INotificationService (Abstract Factory Pattern) pentru a construi
     /// si trimite mesajul pe canalul ales (email/sms).
     ///
     /// Declansatori (NewStatus):
     ///   "Processing"   → NotifyOrderAccepted  (comanda confirmata si in procesare)
     ///   "Shipped"      → NotifyOrderShipped   (comanda expediata, estimare +3 zile)
     ///   "Delivered"    → NotifyOrderDelivered (comanda livrata)
     ///   "Cancelled"    → NotifyOrderCancelled (comanda anulata)
     ///   "PaymentFailed"→ NotifyOrderCancelled (plata esuata, comanda nu va fi procesata)
     ///   Alte statusuri → nicio notificare (ex: "OnHold", "BackOrder")
     ///
     /// Daca RecipientContact lipseste din eveniment, observatorul se sare silentios.
     /// </summary>
     public class CustomerNotificationObserver : IOrderObserver
     {
          private readonly INotificationService _notificationService;

          public string ObserverName => "CustomerNotificationObserver";

          public CustomerNotificationObserver(INotificationService notificationService)
          {
               _notificationService = notificationService;
          }

          public Task OnOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent)
          {
               // Fara date de contact → notificarea se sare
               if (!orderEvent.HasContactData)
               {
                    Console.WriteLine($"[{ObserverName}] Date de contact lipsa — notificarea sarita.");
                    return Task.CompletedTask;
               }

               string channel   = orderEvent.NotificationChannel;
               string recipient = orderEvent.RecipientContact!;
               string name      = orderEvent.RecipientName;
               int    orderId   = orderEvent.OrderId;

               switch (orderEvent.NewStatus.ToLower())
               {
                    case "processing":
                         _notificationService.NotifyOrderAccepted(
                              orderId       : orderId,
                              total         : orderEvent.OrderTotal,
                              recipientName : name,
                              channel       : channel,
                              recipient     : recipient);
                         Console.WriteLine($"[{ObserverName}] ✓ Notificare 'Comanda acceptata' trimisa la {recipient}");
                         break;

                    case "shipped":
                         _notificationService.NotifyOrderShipped(
                              orderId           : orderId,
                              estimatedDelivery : DateTime.UtcNow.AddDays(3),
                              recipientName     : name,
                              channel           : channel,
                              recipient         : recipient);
                         Console.WriteLine($"[{ObserverName}] ✓ Notificare 'Comanda expediata' trimisa la {recipient}");
                         break;

                    case "delivered":
                         _notificationService.NotifyOrderDelivered(
                              orderId       : orderId,
                              recipientName : name,
                              channel       : channel,
                              recipient     : recipient);
                         Console.WriteLine($"[{ObserverName}] ✓ Notificare 'Comanda livrata' trimisa la {recipient}");
                         break;

                    case "cancelled":
                    case "paymentfailed":
                         _notificationService.NotifyOrderCancelled(
                              orderId       : orderId,
                              recipientName : name,
                              channel       : channel,
                              recipient     : recipient);
                         Console.WriteLine($"[{ObserverName}] ✓ Notificare 'Comanda anulata' trimisa la {recipient}");
                         break;

                    default:
                         Console.WriteLine($"[{ObserverName}] Status '{orderEvent.NewStatus}' — fara notificare configurata.");
                         break;
               }

               return Task.CompletedTask;
          }
     }
}
