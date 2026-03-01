using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.Models
{
     public class OrderData
     {
          public int Id { get; set; }
          public string ClientName { get; set; } = "";
          public string ProductList { get; set; } = "";
          public string Status { get; set; } = "";

     }
}
