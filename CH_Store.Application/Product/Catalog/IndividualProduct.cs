using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Catalog
{
     public class IndividualProduct : CatalogComponent
     {
          private readonly ProductPrototypeData _data;

          public IndividualProduct(ProductPrototypeData data)
          {
               _data = data;
          }

          // Returnează numele din clasa de date
          public override string Name => _data.Name;

          // Returnează prețul direct din date
          public override double GetPrice() => _data.Price;

          public override void Display(int depth)
          {
               string indent = new string(' ', depth * 2);
               Console.WriteLine($"{indent}- Product: {Name} | Price: {_data.Price} RON | Weight: {_data.Weight}kg");
          }
     }
}
