using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Catalog
{
     public abstract class CatalogComponent
     {
          // Proprietăți și metode comune tuturor elementelor
          public abstract string Name { get; }
          public abstract double GetPrice();
          public abstract void Display(int depth);
     }
}
