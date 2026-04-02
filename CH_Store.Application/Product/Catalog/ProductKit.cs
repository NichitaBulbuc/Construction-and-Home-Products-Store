using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Catalog
{
     public class ProductKit : CatalogComponent
     {
          // metodă pentru a expune copiii către API
          public List<CatalogComponent> Elements => _children;
          // Câmp privat pentru stocarea sub-elementelor (Componente)
          private readonly List<CatalogComponent> _children = new List<CatalogComponent>();
          private string _kitName;

          public ProductKit(string name)
          {
               _kitName = name;
          }

          public override string Name => _kitName;

          // Metode de management specifice doar pentru Composite (Abordarea Safety)
          public void Add(CatalogComponent component)
          {
               if (component != null)
                    _children.Add(component);
          }

          public void Remove(CatalogComponent component)
          {
               _children.Remove(component);
          }

          public CatalogComponent GetChild(int index)
          {
               return _children[index];
          }

          public List<CatalogComponent> GetAllChildren()
          {
               return _children;
          }

          // DELEGARE: Calculează prețul total însumând prețurile copiilor
          public override double GetPrice()
          {
               return _children.Sum(child => child.GetPrice());
          }

          public override void Display(int depth)
          {
               string indent = new string(' ', depth * 2);
               Console.WriteLine($"{indent}+ KIT: {Name} (Total Kit: {GetPrice()} RON)");

               foreach (var child in _children)
               {
                    // Apel recursiv către copii
                    child.Display(depth + 1);
               }
          }
     }
}
