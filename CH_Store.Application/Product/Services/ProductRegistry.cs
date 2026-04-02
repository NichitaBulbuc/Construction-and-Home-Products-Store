using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Services
{
     public class ProductRegistry
     {
          private Dictionary<string, ProductPrototype> _prototypes = new Dictionary<string, ProductPrototype>();

          public void AddItem(string id, ProductPrototype p)
          {
               _prototypes[id] = p;
          }

          public ProductPrototype GetById(string id)
          {
               // Returnează o clonă, nu obiectul original din registru
               return _prototypes[id].Clone();
          }
     }
}
