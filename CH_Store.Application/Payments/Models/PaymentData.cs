using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Models
{
     public class PaymentData
     {
          public int Id { get; set; }
          public double Amount { get; set; }
          public PaymentType Method { get; set; } // Enum-ul tău (Card, EWallet, etc.)
          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
          public string Status { get; set; } = "Pending"; // Pending, Success, Failed
     }
}
