using CH_Store.Domain.Enums;

namespace CH_Store.Domain.DTOs
{
     public class OrderNotificationRequest
     {
          // ─── Routing ──────────────────────────────────────────────
          /// <summary>Tipul evenimentului pentru care se trimite notificarea.</summary>
          public NotificationEvent Event { get; set; } = NotificationEvent.OrderAccepted;

          /// <summary>Canalul de trimitere: "email" sau "sms".</summary>
          public string Channel { get; set; } = "email";

          /// <summary>Adresa email sau numarul de telefon al destinatarului.</summary>
          public string Recipient { get; set; } = "";

          public string RecipientName { get; set; } = "";

          // ─── Date comenzi ─────────────────────────────────────────
          public int?     OrderId            { get; set; }
          public decimal? OrderTotal         { get; set; }
          public string?  OrderStatus        { get; set; }
          public DateTime? EstimatedDelivery { get; set; }

          // ─── Date promotii ────────────────────────────────────────
          public string?  PromoCode        { get; set; }
          public string?  PromoDescription { get; set; }
          public decimal? DiscountPercent  { get; set; }
     }
}
