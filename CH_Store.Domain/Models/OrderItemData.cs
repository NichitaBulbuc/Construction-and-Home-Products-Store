using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.Models
{
     public class OrderItemData
     {
          public int Id { get; set; }
          public string ProductName { get; set; } = "";
          public double Price { get; set; }
          public int Quantity { get; set; }
     }
}
