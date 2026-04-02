using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.Models
{
     public class ProductPrototypeData
     {
          public int Id { get; set; }
          public string Name { get; set; } = "";
          public string Description { get; set; } = "";
          public double Price { get; set; }
          public double Weight { get; set; }// For construction products
          public string EnergyClass { get; set; } = "";// For Home products
     }
}
