using CH_Store.Domain.Enums;

namespace CH_Store.Domain.Models
{
     /// <summary>
     /// Model de domeniu (in-memory) construit de OrderBuilder.
     /// Nu este entitate EF — persistenta se face prin OrderRepo.
     /// </summary>
     public class OrderData
     {
          public Guid BuilderId { get; set; } = Guid.NewGuid();

          public int UserId { get; set; }

          public List<OrderItemData> Items { get; set; } = new();

          public string DeliveryAddress { get; set; } = "";

          public DeliveryType DeliveryType { get; set; } = DeliveryType.Standard;

          public decimal DeliveryCost { get; set; }

          public bool HasInstallation { get; set; }

          public bool IsPriority { get; set; }

          public decimal Discount { get; set; }

          public string Notes { get; set; } = "";

          public decimal TotalPrice { get; set; }

          public string Status { get; set; } = "New";

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

          // ── Strategy Pattern ─────────────────────────────────────────────────
          /// <summary>Descrierea strategiei de livrare selectate (ex: "Livrare Express — 1-2 zile, +100 MDL").</summary>
          public string DeliveryStrategyDescription { get; set; } = "";

          /// <summary>Numarul estimat de zile pana la livrare, conform strategiei.</summary>
          public int EstimatedDeliveryDays { get; set; }

          /// <summary>Descrierea strategiei de plata utilizate (ex: "Plata cu cardul bancar").</summary>
          public string PaymentStrategyDescription { get; set; } = "";

          /// <summary>Raportul text generat de OrderReportBuilder, salvat in DB.</summary>
          public string ReportSnapshot { get; set; } = "";
     }
}
