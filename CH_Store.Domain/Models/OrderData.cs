using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.Models
{
     public class OrderData
     {
          public Guid Id { get; set; } = Guid.NewGuid();
          public string ClientName { get; set; } = "";
          public List<OrderItemData> Items { get; set; } = new List<OrderItemData>();
          public string DeliveryAddress { get; set; } = "";
          public bool HasInstallation { get; set; }
          public bool IsPriority { get; set; }
          public double TotalPrice { get; set; }
          public string Status { get; set; } = "";

     }
}
