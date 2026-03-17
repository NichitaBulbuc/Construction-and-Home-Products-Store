using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Services
{
     public class HomeProduct : ProductPrototype
     {
          public HomeProduct(ProductPrototypeData data) : base(data) { }
          public override ProductPrototype Clone()
          {
               var dataCopy = new ProductPrototypeData
               {
                    Name = this._data.Name,
                    Price = this._data.Price,
                    Description = this._data.Description,
                    EnergyClass = this._data.EnergyClass
               };
               return new HomeProduct(dataCopy);
          }

          public override void DisplayInfo()
          {
               base.DisplayInfo();
               Console.WriteLine($" -> Home Type, Energy Class: {_data.EnergyClass}");
          }
     }
}
