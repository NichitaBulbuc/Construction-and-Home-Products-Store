using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs.Auth
{
     public class LoginRequest
     {
          [Required]
          [StringLength(30, MinimumLength = 3)]
          public string Username { get; set; } = "";

          [Required]
          [StringLength(100, MinimumLength = 6)]
          public string Password { get; set; } = "";
     }
}
