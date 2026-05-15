using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CH_Store.Domain.Entities
{
     /// <summary>
     /// Entitate DB pentru nodul Composite (kit / categorie de produse).
     /// Un kit poate contine produse individuale si/sau alte kituri (imbricare recursiva).
     /// </summary>
     public class CatalogKitDbTable
     {
          [Key]
          [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
          public int Id { get; set; }

          [Required]
          [StringLength(150, MinimumLength = 2)]
          public string Name { get; set; } = "";

          [StringLength(500)]
          public string? Description { get; set; }

          public bool IsActive { get; set; } = true;

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

          // ─── Navigation ───────────────────────────────────────────────────────
          /// <summary>Elementele din acest kit (produse sau sub-kituri).</summary>
          public ICollection<CatalogKitItemDbTable> Items { get; set; } = new List<CatalogKitItemDbTable>();
     }
}
