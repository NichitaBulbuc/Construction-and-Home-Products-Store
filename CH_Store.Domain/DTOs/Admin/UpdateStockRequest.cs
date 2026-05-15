using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs.Admin
{
     public class UpdateStockRequest
     {
          /// <summary>Noua cantitate absoluta in stoc (nu delta).</summary>
          [Required, Range(0, 100_000)]
          public int StockQuantity { get; set; }

          public string? Reason { get; set; }
     }
}
