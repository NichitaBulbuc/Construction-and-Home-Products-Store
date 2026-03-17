using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.DTOs
{
     public class ProductResponse
     {
          public int Id { get; set; }
          public string Name { get; set; } = "";
          public string Description { get; set; } = "";
          public double Price { get; set; }
          public string CategoryInfo { get; set; } = ""; // Weight sau EnergyClass formatat

     }
}
