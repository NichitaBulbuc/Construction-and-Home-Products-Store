using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;

namespace CH_Store.Application.Product.Catalog
{
     /// <summary>
     /// Composite Pattern — Leaf.
     ///
     /// Reprezinta un produs individual din catalogul CH Store.
     /// Wrapper peste <see cref="ProductDbTable"/> (entitatea SQL Server din AppDbContext).
     /// Nu are copii — este nodul terminal al arborelui.
     ///
     /// Inainte: wrapa ProductPrototypeData (InMemory) → rupt de baza de date reala.
     /// Acum:    wrapa ProductDbTable (SQL Server AppDbContext) → date reale, persistate.
     /// </summary>
     public class IndividualProduct : CatalogComponent
     {
          private readonly ProductDbTable _product;
          private readonly int            _quantity;

          /// <param name="product">Entitatea din AppDbContext.Products (SQL Server).</param>
          /// <param name="quantity">Cantitatea acestui produs in contextul kitului parinte.</param>
          public IndividualProduct(ProductDbTable product, int quantity = 1)
          {
               _product  = product;
               _quantity = quantity > 0 ? quantity : 1;
          }

          // ─── Component interface ──────────────────────────────────────────────
          public override string Name => _product.Name;
          public override string Type => "Product";

          /// <summary>Pret unitar × cantitate.</summary>
          public override double GetPrice() => _product.Price * _quantity;

          /// <summary>Leaf: returneaza cantitatea sa proprie.</summary>
          public override int GetProductCount() => _quantity;

          public override CatalogNodeDto ToCatalogNodeDto() => new()
          {
               Id           = _product.Id,
               Name         = _product.Name,
               Type         = "Product",
               UnitPrice    = _product.Price,
               Quantity     = _quantity,
               TotalPrice   = GetPrice(),
               Description  = _product.Description,
               Category     = _product.Category,
               StockQuantity = _product.StockQuantity,
               Weight       = _product.Weight,
               EnergyClass  = _product.EnergyClass,
               Children     = new List<CatalogNodeDto>()
          };

          public override void Display(int depth = 0)
          {
               string indent = new(' ', depth * 2);
               Console.WriteLine($"{indent}▪ {Name} | {_product.Price:F2} MDL x {_quantity} = {GetPrice():F2} MDL" +
                                 $" | Stoc: {_product.StockQuantity}" +
                                 (_product.Weight > 0 ? $" | {_product.Weight}kg" : "") +
                                 (!string.IsNullOrEmpty(_product.EnergyClass) ? $" | {_product.EnergyClass}" : ""));
          }
     }
}
