using CH_Store.Domain.Enums;

namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// DTO pentru plasarea unei comenzi complete cu plata integrata si notificare.
     /// Extinde OrderRequest adaugand:
     ///   - metoda de plata (Factory Method → PaymentProvider)
     ///   - datele de contact pentru notificare (Abstract Factory → NotificationService)
     /// </summary>
     public class OrderWithPaymentRequest : OrderRequest
     {
          /// <summary>Metoda de plata aleasa de client.</summary>
          public PaymentType PaymentMethod { get; set; } = PaymentType.Card;

          /// <summary>Canalul de notificare: "email" sau "sms".</summary>
          public string NotificationChannel { get; set; } = "email";

          /// <summary>Adresa email sau numarul de telefon al clientului.</summary>
          public string RecipientContact { get; set; } = "";

          /// <summary>Numele clientului pentru personalizarea notificarii.</summary>
          public string RecipientName { get; set; } = "";
     }
}
