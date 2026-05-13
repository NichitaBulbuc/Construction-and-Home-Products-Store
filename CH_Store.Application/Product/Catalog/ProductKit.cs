using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Product.Catalog
{
     /// <summary>
     /// Composite Pattern — Composite.
     ///
     /// Un kit de produse care poate contine:
     ///   • IndividualProduct (Leaf)  — produse individuale
     ///   • ProductKit (Composite)    — sub-kituri imbricaterecursiv
     ///
     /// Operatiile (GetPrice, GetProductCount, ToCatalogNodeDto) se propaga
     /// recursiv la toti copiii — clientul nu stie daca lucreaza cu un
     /// produs simplu sau un kit complex.
     ///
     /// Persistat in baza de date via CatalogKitDbTable + CatalogKitItemDbTable.
     /// CatalogService construieste arborele din DB si il returneaza ca ProductKit.
     /// </summary>
     public class ProductKit : CatalogComponent
     {
          private readonly int    _kitId;
          private readonly string _kitName;
          private readonly string? _description;

          // Abordarea Safety: metodele de management (Add/Remove) sunt pe Composite, nu pe Component
          private readonly List<CatalogComponent> _children = new();

          public ProductKit(int kitId, string name, string? description = null)
          {
               _kitId       = kitId;
               _kitName     = name;
               _description = description;
          }

          // ─── Component interface ──────────────────────────────────────────────
          public override string Name => _kitName;
          public override string Type => "Kit";

          /// <summary>ID-ul kitului din CatalogKitDbTable (SQL Server).</summary>
          public int KitId => _kitId;

          /// <summary>Delegare: suma preturilor tuturor copiilor (recursiv).</summary>
          public override double GetPrice()
               => _children.Sum(c => c.GetPrice());

          /// <summary>Delegare: numarul total de produse individuale din intreg sub-arborele.</summary>
          public override int GetProductCount()
               => _children.Sum(c => c.GetProductCount());

          public override CatalogNodeDto ToCatalogNodeDto() => new()
          {
               Id           = _kitId,
               Name         = _kitName,
               Type         = "Kit",
               UnitPrice    = 0,
               TotalPrice   = GetPrice(),
               Description  = _description,
               ProductCount = GetProductCount(),
               Children     = _children.Select(c => c.ToCatalogNodeDto()).ToList()
          };

          public override void Display(int depth = 0)
          {
               string indent = new(' ', depth * 2);
               Console.WriteLine($"{indent}▶ KIT: {_kitName}" +
                                 $" | Total: {GetPrice():F2} MDL" +
                                 $" | {GetProductCount()} produs(e)");

               foreach (var child in _children)
                    child.Display(depth + 1);
          }

          // ─── Composite-only: management copii (Safety approach) ──────────────

          public void Add(CatalogComponent component)
          {
               if (component != null)
                    _children.Add(component);
          }

          public void Remove(CatalogComponent component)
               => _children.Remove(component);

          public CatalogComponent? GetChild(int index)
               => index >= 0 && index < _children.Count ? _children[index] : null;

          public IReadOnlyList<CatalogComponent> GetChildren()
               => _children.AsReadOnly();
     }
}
