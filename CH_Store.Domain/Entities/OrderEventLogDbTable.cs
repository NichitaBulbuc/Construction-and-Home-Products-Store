using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CH_Store.Domain.Entities
{
     /// <summary>
     /// Observer Pattern — entitate DB pentru AdminDashboardObserver.
     /// Fiecare schimbare de status a unei comenzi genereaza o inregistrare.
     /// Folosita pentru audit trail si dashboard admin.
     /// </summary>
     public class OrderEventLogDbTable
     {
          [Key]
          [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
          public int Id { get; set; }

          [Required]
          public int OrderId { get; set; }

          [Required]
          public int UserId { get; set; }

          [Required]
          [StringLength(30)]
          public string OldStatus { get; set; } = "";

          [Required]
          [StringLength(30)]
          public string NewStatus { get; set; } = "";

          [Column(TypeName = "decimal(18,2)")]
          public decimal OrderTotal { get; set; }

          public int ItemsCount { get; set; }

          [StringLength(300)]
          public string? Notes { get; set; }

          public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

          // ─── Navigation ───────────────────────────────────────────────────────
          [ForeignKey("OrderId")]
          public virtual OrderDbTable Order { get; set; } = null!;
     }
}
