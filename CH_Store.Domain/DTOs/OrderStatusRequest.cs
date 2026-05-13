namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// DTO pentru actualizarea statusului unei comenzi.
     /// Datele de notificare sunt optionale — daca lipsesc, notificarea este sarita.
     /// </summary>
     public class OrderStatusRequest
     {
          /// <summary>Noul status: "Processing", "Shipped", "Delivered", "Cancelled".</summary>
          public string NewStatus { get; set; } = "";

          /// <summary>Motivul schimbarii (optional, afisat in notificare si log).</summary>
          public string? Reason { get; set; }

          // ─── Date optionale de notificare ────────────────────────────────────
          /// <summary>"email" sau "sms". Null = fara notificare.</summary>
          public string? NotificationChannel { get; set; }

          /// <summary>Adresa email sau numarul de telefon.</summary>
          public string? RecipientContact { get; set; }

          /// <summary>Numele clientului pentru personalizarea mesajului.</summary>
          public string? RecipientName { get; set; }

          /// <summary>True daca toate campurile de notificare sunt completate.</summary>
          public bool HasNotificationData =>
               !string.IsNullOrWhiteSpace(NotificationChannel) &&
               !string.IsNullOrWhiteSpace(RecipientContact);
     }
}
