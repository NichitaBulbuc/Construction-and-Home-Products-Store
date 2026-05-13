using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs.Admin
{
     public class AdminRejectOrderRequest
     {
          [Required]
          public string Reason { get; set; } = "";

          public string? RecipientContact    { get; set; }
          public string? RecipientName       { get; set; }
          public string  NotificationChannel { get; set; } = "email";
     }
}
