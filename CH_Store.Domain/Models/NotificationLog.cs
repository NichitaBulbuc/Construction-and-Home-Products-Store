using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.Models
{
     /// <summary>
     /// Inregistrare a fiecarei notificari trimise — salvata in NotificationContext (InMemory).
     /// </summary>
     public class NotificationLog
     {
          [Key]
          public int    Id        { get; set; }

          /// <summary>"email" sau "sms"</summary>
          public string Channel   { get; set; } = "";

          /// <summary>Adresa email sau numarul de telefon al destinatarului.</summary>
          public string Recipient { get; set; } = "";

          /// <summary>Tipul evenimentului (OrderAccepted, Promotion, etc.)</summary>
          public string EventType { get; set; } = "";

          /// <summary>Continutul mesajului trimis.</summary>
          public string Content   { get; set; } = "";

          public DateTime SentAt  { get; set; } = DateTime.UtcNow;

          public bool Success     { get; set; } = true;
     }
}
