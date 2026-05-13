using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs.Admin
{
     public class CreateProductRequest
     {
          [Required, StringLength(100, MinimumLength = 2)]
          public string Name { get; set; } = "";

          [StringLength(500)]
          public string Description { get; set; } = "";

          [Required, Range(0.01, 100_000)]
          public double Price { get; set; }

          [Required, Range(0, 100_000)]
          public int StockQuantity { get; set; }

          /// <summary>"Construction" sau "Home"</summary>
          [Required, StringLength(50)]
          public string Category { get; set; } = "";

          public double? Weight      { get; set; }
          public string? EnergyClass { get; set; }
     }
}
