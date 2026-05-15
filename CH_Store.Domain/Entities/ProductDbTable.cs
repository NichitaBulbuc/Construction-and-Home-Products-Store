using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.Entities
{
     public class ProductDbTable
     {
          [Key]
          [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
          public int Id { get; set; }

          [Required]
          [StringLength(100, MinimumLength = 2)]
          [Display(Name = "Product Name")]
          public string Name { get; set; } = null!;

          [StringLength(500)]
          public string Description { get; set; } = "";

          [Required]
          [Range(0.01, 100000.00)]
          [DataType(DataType.Currency)]
          public double Price { get; set; }

          [Required]
          [Display(Name = "Stock Quantity")]
          public int StockQuantity { get; set; }

          [StringLength(50)]
          public string Category { get; set; } = ""; // "Construction" sau "Home"

          // Câmpuri opționale pentru flexibilitate 
          public double? Weight { get; set; }
          public string? EnergyClass { get; set; }

          [Display(Name = "Added Date")]
          public DateTime CreatedDateTime { get; set; } = DateTime.Now;
     }
}
