namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// DTO pentru un nod din arborele Composite al catalogului.
     /// Folosit atat pentru Leaf (Product) cat si pentru Composite (Kit).
     /// Type discriminator: "Product" | "Kit"
     /// </summary>
     public class CatalogNodeDto
     {
          public int    Id   { get; set; }
          public string Name { get; set; } = "";
          public string Type { get; set; } = ""; // "Product" | "Kit"

          // ─── Pret ─────────────────────────────────────────────────────────────
          /// <summary>Pret unitar (relevant pentru Product; 0 pentru Kit).</summary>
          public double UnitPrice  { get; set; }

          /// <summary>Cantitate in contextul kitului parinte (relevant pentru Product).</summary>
          public int    Quantity   { get; set; } = 1;

          /// <summary>Total = UnitPrice × Quantity (Product) sau suma copiilor (Kit).</summary>
          public double TotalPrice { get; set; }

          // ─── Detalii produs (completate doar pentru Type = "Product") ─────────
          public string? Description  { get; set; }
          public string? Category     { get; set; }
          public int?    StockQuantity { get; set; }
          public double? Weight       { get; set; }
          public string? EnergyClass  { get; set; }

          // ─── Detalii kit (completate doar pentru Type = "Kit") ───────────────
          /// <summary>Numarul total de produse individuale (recursiv).</summary>
          public int ProductCount { get; set; }

          /// <summary>Copiii nodului (empty pentru Leaf, populated pentru Composite).</summary>
          public List<CatalogNodeDto> Children { get; set; } = new();
     }
}
