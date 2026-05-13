using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// Raspuns complet pentru o comanda cu plata integrata.
     /// Contine datele comenzii, rezultatul platii si statusul notificarii.
     /// </summary>
     public class OrderWithPaymentResponse
     {
          // ─── Date comanda (identice cu OrderResponse) ────────────────────────
          public int    DbId      { get; set; }
          public Guid   BuilderId { get; set; }
          public int    UserId    { get; set; }

          public List<OrderItemData> Items { get; set; } = new();

          public string       DeliveryAddress { get; set; } = "";
          public DeliveryType DeliveryType    { get; set; }
          public decimal      DeliveryCost    { get; set; }
          public bool         HasInstallation { get; set; }
          public bool         IsPriority      { get; set; }
          public decimal      Discount        { get; set; }
          public string       Notes           { get; set; } = "";
          public decimal      TotalPrice      { get; set; }
          public string       Status          { get; set; } = "";
          public DateTime     CreatedAt       { get; set; }
          public string       Report          { get; set; } = "";

          // ─── Strategy Pattern — metadata livrare ─────────────────────────────
          public string DeliveryStrategyDescription { get; set; } = "";
          public int    EstimatedDeliveryDays       { get; set; }

          // ─── Rezultat plata ───────────────────────────────────────────────────
          public bool   PaymentSuccess             { get; set; }
          public string PaymentTransactionId       { get; set; } = "";
          public string PaymentMessage             { get; set; } = "";
          public string PaymentMethod              { get; set; } = "";
          public string PaymentStrategyDescription { get; set; } = "";

          // ─── Notificare ───────────────────────────────────────────────────────
          public bool   NotificationSent    { get; set; }
          public string NotificationChannel { get; set; } = "";

          // ─── Chain of Responsibility — rezultatul validarii ───────────────────
          /// <summary>
          /// Rezultatul lantului de aprobare.
          /// Null daca comanda a fost respinsa inainte de a ajunge in DB.
          /// </summary>
          public ApprovalResult? ApprovalResult { get; set; }
     }
}
