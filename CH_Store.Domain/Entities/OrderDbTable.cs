using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.Entities
{
     public class OrderDbTable
     {
          [Key]
          [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
          public int Id { get; set; }

          [Required]
          public int UserId { get; set; }

          [ForeignKey("UserId")]
          public virtual UDbTable User { get; set; } // Relație cu tabelul de Useri

          [Required]
          [Display(Name = "Order Date")]
          public DateTime OrderDate { get; set; } = DateTime.Now;

          [Required]
          [DataType(DataType.Currency)]
          public decimal TotalAmount { get; set; }

          [Required]
          [StringLength(20)]
          public string Status { get; set; } // ex: "New", "Paid", "Shipped"

          [Display(Name = "Shipping Address")]
          [Required]
          public string ShippingAddress { get; set; }
     }
}
