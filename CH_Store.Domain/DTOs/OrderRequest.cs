using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.DTOs
{
     public class OrderRequest
     {
          public int Id { get; set; }
          public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
          public decimal Price { get; set; }
          public string DeliveryAddress { get; set; } = "";
          public bool WantsInstallation { get; set; }
          public bool IsPriority { get; set; }
     }
}
