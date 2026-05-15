using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs
{
     public class AddProductToKitRequest
     {
          [Required]
          public int ProductId { get; set; }

          [Range(1, 10000)]
          public int Quantity { get; set; } = 1;

          public int SortOrder { get; set; } = 0;
     }
}
