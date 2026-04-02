using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Services
{
     public abstract class ProductPrototype
     {
          protected ProductPrototypeData _data;
          public ProductPrototypeData Data => _data;
          public ProductPrototype(ProductPrototypeData data) 
          { 
               _data = data;
          }

          public abstract ProductPrototype Clone();
          public virtual void DisplayInfo()
          {
               Console.WriteLine($"Product: {_data.Name}, Price: {_data.Price}");
          }

     }
}
