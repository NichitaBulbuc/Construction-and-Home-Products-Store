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
          public virtual UDbTable User { get; set; } = null!;

          [Required]
          [Display(Name = "Order Date")]
          public DateTime OrderDate { get; set; } = DateTime.UtcNow;

          [Required]
          [Column(TypeName = "decimal(18,2)")]
          [DataType(DataType.Currency)]
          public decimal TotalAmount { get; set; }

          [Required]
          [StringLength(20)]
          public string Status { get; set; } = "New"; // "New","Paid","Processing","Shipped","Delivered","Cancelled"

          [Required]
          [Display(Name = "Shipping Address")]
          public string ShippingAddress { get; set; } = "";

          // --- Livrare ---
          [StringLength(20)]
          public string DeliveryType { get; set; } = "Standard";

          [Column(TypeName = "decimal(18,2)")]
          public decimal DeliveryCost { get; set; }

          // --- Optiuni suplimentare ---
          public bool HasInstallation { get; set; }

          public bool IsPriority { get; set; }

          [Column(TypeName = "decimal(18,2)")]
          public decimal Discount { get; set; }

          [StringLength(500)]
          public string? Notes { get; set; }

          // --- Raport generat de OrderReportBuilder ---
          public string? ReportSnapshot { get; set; }

          // --- Navigatie spre produsele din comanda ---
          public virtual ICollection<OrderItemDbTable> Items { get; set; } = new List<OrderItemDbTable>();
     }
}
