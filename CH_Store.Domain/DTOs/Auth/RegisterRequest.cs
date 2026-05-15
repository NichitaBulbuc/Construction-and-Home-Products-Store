using System.ComponentModel.DataAnnotations;

namespace CH_Store.Domain.DTOs.Auth
{
     public class RegisterRequest
     {
          [Required]
          [StringLength(30, MinimumLength = 3)]
          public string Username { get; set; } = "";

          [Required]
          [EmailAddress]
          [StringLength(30)]
          public string Email { get; set; } = "";

          [Required]
          [StringLength(100, MinimumLength = 6)]
          public string Password { get; set; } = "";

          [StringLength(30)]
          public string? Phone { get; set; }

          public string? Address { get; set; }
     }
}
