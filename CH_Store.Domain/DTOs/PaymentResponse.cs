namespace CH_Store.Domain.DTOs
{
     public class PaymentResponse
     {
          public bool     Success       { get; set; }
          public string   Message       { get; set; } = "";
          public string   TransactionId { get; set; } = "";
          public double   Amount        { get; set; }
          public string   PaymentMethod { get; set; } = "";
          public DateTime ProcessedAt   { get; set; }
     }
}
