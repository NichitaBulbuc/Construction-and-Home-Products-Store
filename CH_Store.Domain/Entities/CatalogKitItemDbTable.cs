using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CH_Store.Domain.Entities
{
     /// <summary>
     /// Entitate DB pentru un element dintr-un kit (Composite Pattern — legatura parinte-copil).
     ///
     /// Fiecare inregistrare reprezinta UNUL dintre:
     ///   • Un produs individual (ProductId != null, SubKitId == null)  → Leaf
     ///   • Un sub-kit         (SubKitId  != null, ProductId == null)  → Composite copil
     ///
     /// Aceasta structura permite imbricarea recursiva a kiturilor (arbore de adancime N).
     /// </summary>
     public class CatalogKitItemDbTable
     {
          [Key]
          [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
          public int Id { get; set; }

          // ─── FK: Kitul parinte ────────────────────────────────────────────────
          [Required]
          public int KitId { get; set; }

          // ─── Discriminator: produs SAU sub-kit (exact unul dintre ele e completat) ─
          /// <summary>FK catre ProductDbTable. Completat cand elementul este un produs (Leaf).</summary>
          public int? ProductId { get; set; }

          /// <summary>FK catre CatalogKitDbTable. Completat cand elementul este un sub-kit (Composite).</summary>
          public int? SubKitId { get; set; }

          /// <summary>Cantitatea produsului in kit. Relevant doar cand ProductId != null.</summary>
          [Range(1, 10000)]
          public int Quantity { get; set; } = 1;

          /// <summary>Ordine de afisare in interiorul kitului.</summary>
          public int SortOrder { get; set; } = 0;

          // ─── Navigation ───────────────────────────────────────────────────────
          public CatalogKitDbTable  Kit     { get; set; } = null!;
          public ProductDbTable?    Product { get; set; }
          public CatalogKitDbTable? SubKit  { get; set; }
     }
}
