using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs
{
     public class CreateKitRequest
     {
          [Required]
          [StringLength(150, MinimumLength = 2)]
          public string Name { get; set; } = "";

          [StringLength(500)]
          public string? Description { get; set; }
     }
}
