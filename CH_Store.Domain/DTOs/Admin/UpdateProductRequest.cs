using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs.Admin
{
     /// <summary>Câmpuri null-able → doar câmpurile trimise sunt actualizate (PATCH-style în PUT).</summary>
     public class UpdateProductRequest
     {
          [StringLength(100, MinimumLength = 2)]
          public string? Name { get; set; }

          [StringLength(500)]
          public string? Description { get; set; }

          [Range(0.01, 100_000)]
          public double? Price { get; set; }

          [StringLength(50)]
          public string? Category { get; set; }

          public double? Weight      { get; set; }
          public string? EnergyClass { get; set; }
     }
}
