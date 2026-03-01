using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.DTOs
{
     public class OrderNotificationRequest
     {
          public int OrderId { get; set; }
          public string? CustomerPhone { get; set; }
          public string? CustomerEmail { get; set; }
          public string? NotificationType { get; set; } = "sms";
     }
}
