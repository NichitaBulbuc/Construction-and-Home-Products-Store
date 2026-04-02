using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.DTOs
{
     public class ProductRequest
     {
          public string PrototypeType { get; set; } = ""; // "construction" sau "home"
          public string CustomName { get; set; } = "";
          public double? OverriddenPrice { get; set; } // Opțional, dacă vrem să schimbăm prețul prototipului
     }
}
