using CH_Store.Domain.Enums;

namespace CH_Store.Domain.DTOs.Admin
{
     public class AdminApproveOrderRequest
     {
          public string? Notes              { get; set; }
          public string? RecipientContact   { get; set; }
          public string? RecipientName      { get; set; }
          public string  NotificationChannel { get; set; } = "email";

          /// <summary>
          /// Daca specificat, adminul initiaza si plata acum (ex: comanda B2B aprobata manual).
          /// Daca null, statusul devine direct "Processing" fara plata electronica.
          /// </summary>
          public PaymentType? ProcessPaymentWith { get; set; }
     }
}
