using CH_Store.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.Entities
{
     public class UDbTable
     {
          [Key]
          [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
          public int Id { get; set; }

          [Required]
          [Display(Name = "Username")]
          [StringLength(30, MinimumLength = 3, ErrorMessage = "Username cannot be longer than 30 characters.")]
          public string Username { get; set; }

          [Required]
          [Display(Name = "Password")]
          [StringLength(50, MinimumLength = 8, ErrorMessage = "Password cannot be shorter than 8 characters.")]
          public string Password { get; set; }

          [Required]
          [Display(Name = "Email")]
          [StringLength(30)]
          public string Email { get; set; }

          [Display(Name = "Address")]
          public string Address { get; set; }

          [Display(Name = "Phone")]
          public string Phone { get; set; }

          [StringLength(30)]
          [Display(Name = "UserIP")]
          public string UserIP { get; set; }

          [Display(Name = "Login_dt")]
          [DataType(DataType.Date)]
          public DateTime LastLoginDateTime { get; set; }

          [Display(Name = "Reg_dt")]
          [DataType(DataType.Date)]
          public DateTime RegistartionDateTime { get; set; }

          [Display(Name = "U_Level")]
          public URole Level { get; set; }
     }
}
