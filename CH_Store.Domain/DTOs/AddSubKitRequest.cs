using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs
{
     public class AddSubKitRequest
     {
          [Required]
          public int SubKitId { get; set; }

          public int SortOrder { get; set; } = 0;
     }
}
