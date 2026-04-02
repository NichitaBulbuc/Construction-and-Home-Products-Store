using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Services
{
     public class ConstructionProduct : ProductPrototype
     {
          public ConstructionProduct(ProductPrototypeData data): base(data) { }
          public override ProductPrototype Clone()
          {
               var dataCopy = new ProductPrototypeData {
                    Name = this._data.Name,
                    Price = this._data.Price,
                    Description = this._data.Description,
                    Weight = this._data.Weight
               };
               return new ConstructionProduct(dataCopy);
          }
          public override void DisplayInfo()
          {
               base.DisplayInfo();
               Console.WriteLine($" -> Construction Type, Weight: {_data.Weight}kg");
          }


     }
}
