namespace CH_Store.Domain.Models
{
     /// <summary>
     /// Rezultatul simulat returnat de fiecare IPaymentProcessor.
     /// Nu reprezinta o tranzactie reala — doar simuleaza mesajul metodei de plata.
     /// </summary>
     public class PaymentResult
     {
          public bool   Success       { get; set; }
          public string Message       { get; set; } = "";
          public string TransactionId { get; set; } = "";
          public double Amount        { get; set; }
          public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
     }
}
