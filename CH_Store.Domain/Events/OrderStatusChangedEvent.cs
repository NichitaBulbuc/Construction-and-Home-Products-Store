namespace CH_Store.Domain.Events
{
     /// <summary>
     /// Observer Pattern — Event payload.
     ///
     /// Transporta toate datele unui eveniment de schimbare a statusului comenzii.
     /// Publicat de Subject (OrderEventPublisher) si consumat de fiecare Observer.
     ///
     /// Contine:
     ///   • Date de identificare (OrderId, UserId, totale)
     ///   • Tranzitia de status (OldStatus → NewStatus)
     ///   • Produsele comenzii (pentru StockObserver — actualizare stocuri)
     ///   • Date de contact optionale (pentru CustomerNotificationObserver)
     /// </summary>
     public class OrderStatusChangedEvent
     {
          // ─── Identificare ─────────────────────────────────────────────────────
          public int     OrderId    { get; set; }
          public int     UserId     { get; set; }
          public decimal OrderTotal { get; set; }

          // ─── Tranzitie status ────────────────────────────────────────────────
          public string OldStatus { get; set; } = "";
          public string NewStatus { get; set; } = "";

          // ─── Produse din comanda (pentru StockObserver) ──────────────────────
          public List<OrderEventItem> Items { get; set; } = new();

          // ─── Date de contact (pentru CustomerNotificationObserver) ───────────
          /// <summary>Adresa email sau nr. de telefon. Null = notificarea se sare.</summary>
          public string? RecipientContact    { get; set; }
          public string  RecipientName       { get; set; } = "";
          public string  NotificationChannel { get; set; } = "email";

          // ─── Metadata ────────────────────────────────────────────────────────
          public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

          // ─── Helper ──────────────────────────────────────────────────────────
          public bool HasContactData => !string.IsNullOrWhiteSpace(RecipientContact);
     }

     /// <summary>Produs minimal din eveniment — folosit de StockObserver.</summary>
     public record OrderEventItem(int ProductId, string ProductName, int Quantity);
}
