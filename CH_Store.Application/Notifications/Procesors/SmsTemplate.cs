using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Notifications.Procesors
{
     /// <summary>
     /// Concrete Product B2 — genereaza SMS scurt (max ~160 caractere) pentru fiecare eveniment.
     /// GetSubject returneaza string gol — SMS nu are camp de subiect.
     /// </summary>
     public class SmsTemplate : ITemplateProvider
     {
          public string GetSubject(NotificationData data) => string.Empty;

          public string GetContent(NotificationData data) => data.Event switch
          {
               NotificationEvent.OrderAccepted =>
                    $"CH Store: Comanda #{data.OrderId} ({data.OrderTotal:F2} MDL) acceptata. Multumim!",

               NotificationEvent.OrderProcessing =>
                    $"CH Store: Comanda #{data.OrderId} se proceseaza. Veti fi notificat la expediere.",

               NotificationEvent.OrderShipped =>
                    $"CH Store: Comanda #{data.OrderId} expediata! Livrare estimata: {data.EstimatedDelivery:dd.MM.yyyy}.",

               NotificationEvent.OrderDelivered =>
                    $"CH Store: Comanda #{data.OrderId} livrata. Va multumim!",

               NotificationEvent.OrderCancelled =>
                    $"CH Store: Comanda #{data.OrderId} anulata. Contactati-ne pentru detalii.",

               NotificationEvent.PromotionAvailable =>
                    $"CH Store: {data.PromoDescription} Reducere {data.DiscountPercent}%. Cod: {data.PromoCode}",

               NotificationEvent.SeasonalSale =>
                    $"CH Store: Vanzare sezoniera! Reduceri pana la {data.DiscountPercent}%. Vizitati site-ul.",

               NotificationEvent.Welcome =>
                    $"CH Store: Bun venit, {data.RecipientName}! Contul dvs. a fost activat.",

               NotificationEvent.PasswordReset =>
                    $"CH Store: Cod resetare parola: {data.ResetLink} (valid 30 min).",

               _ =>
                    $"CH Store: Notificare pentru {data.RecipientName}."
          };
     }
}
