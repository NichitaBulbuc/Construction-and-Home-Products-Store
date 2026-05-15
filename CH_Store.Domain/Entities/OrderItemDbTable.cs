using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CH_Store.Domain.Entities
{
     public class OrderItemDbTable
     {
          [Key]
          [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
          public int Id { get; set; }

          [Required]
          public int OrderId { get; set; }

          [ForeignKey("OrderId")]
          public virtual OrderDbTable Order { get; set; } = null!;

          [Required]
          public int ProductId { get; set; }

          [ForeignKey("ProductId")]
          public virtual ProductDbTable Product { get; set; } = null!;

          [Required]
          [StringLength(100)]
          public string ProductName { get; set; } = "";

          [Required]
          [Column(TypeName = "decimal(18,2)")]
          public decimal UnitPrice { get; set; }

          [Required]
          [Range(1, 100000)]
          public int Quantity { get; set; }

          [Required]
          [Column(TypeName = "decimal(18,2)")]
          public decimal Subtotal { get; set; }
     }
}
