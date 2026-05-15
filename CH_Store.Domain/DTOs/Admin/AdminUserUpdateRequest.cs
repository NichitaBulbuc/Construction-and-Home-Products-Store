using CH_Store.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs.Admin
{
     public class AdminUserUpdateRequest
     {
          [StringLength(30)]
          public string? Email   { get; set; }

          [StringLength(30)]
          public string? Phone   { get; set; }

          public string? Address { get; set; }

          public URole? Level    { get; set; }
     }
}
