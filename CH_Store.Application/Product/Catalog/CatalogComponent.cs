using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Product.Catalog
{
     /// <summary>
     /// Composite Pattern — Component (interfata uniforma).
     ///
     /// Atat produsele individuale (Leaf) cat si kiturile (Composite)
     /// implementeaza aceasta clasa abstracta. Clientul nu stie si nu
     /// trebuie sa stie cu ce tip de nod lucreaza.
     ///
     ///   Leaf      = IndividualProduct  (produs simplu din ProductDbTable)
     ///   Composite = ProductKit          (kit care contine Leaf-uri si/sau alte kituri)
     /// </summary>
     public abstract class CatalogComponent
     {
          // ─── Interfata uniforma (tratata identic de client) ─────────────────
          public abstract string Name        { get; }
          public abstract string Type        { get; }   // "Product" | "Kit"

          /// <summary>Pret total al nodului (produs: pret * cantitate; kit: suma copiilor).</summary>
          public abstract double GetPrice();

          /// <summary>Numarul total de produse individuale (recursiv pentru kituri).</summary>
          public abstract int GetProductCount();

          /// <summary>
          /// Construieste DTO-ul structurat pentru API — inlocuieste Display(depth).
          /// Kiturile includ recursiv copiii in Children.
          /// </summary>
          public abstract CatalogNodeDto ToCatalogNodeDto();

          /// <summary>
          /// Afisare la consola cu indentare (util pentru debugging si logging).
          /// Pastrat pentru compatibilitate si tracing.
          /// </summary>
          public abstract void Display(int depth = 0);
     }
}
