using CH_Store.Domain.Enums;

namespace CH_Store.Domain.Models
{
     /// <summary>
     /// Contextul complet transmis catre ITemplateProvider.
     /// Fiecare template genereaza continut pe baza EventType-ului si campurilor disponibile.
     /// </summary>
     public class NotificationData
     {
          // ─── Obligatoriu ───────────────────────────────────────────
          public NotificationEvent Event         { get; set; }
          public string            RecipientName { get; set; } = "";

          // ─── Comenzi (utilizat la OrderAccepted, Shipped, etc.) ───
          public int?     OrderId           { get; set; }
          public decimal? OrderTotal        { get; set; }
          public string?  OrderStatus       { get; set; }
          public DateTime? EstimatedDelivery { get; set; }

          // ─── Promotii ──────────────────────────────────────────────
          public string? PromoCode        { get; set; }
          public string? PromoDescription { get; set; }
          public decimal? DiscountPercent { get; set; }

          // ─── Cont ──────────────────────────────────────────────────
          public string? ResetLink { get; set; }
     }
}
