using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Domain.DTOs
{
     public class OrderResponse
     {
          /// <summary>ID generat de SQL Server (identity).</summary>
          public int DbId { get; set; }

          /// <summary>GUID intern al builder-ului pentru tracing.</summary>
          public Guid BuilderId { get; set; }

          public int UserId { get; set; }

          public List<OrderItemData> Items { get; set; } = new();

          public string DeliveryAddress { get; set; } = "";

          public DeliveryType DeliveryType { get; set; }

          public decimal DeliveryCost { get; set; }

          public bool HasInstallation { get; set; }

          public bool IsPriority { get; set; }

          public decimal Discount { get; set; }

          public string Notes { get; set; } = "";

          public decimal TotalPrice { get; set; }

          public string Status { get; set; } = "";

          public DateTime CreatedAt { get; set; }

          public string Report { get; set; } = "";

          // ─── Strategy Pattern — metadata livrare ─────────────────────────────
          /// <summary>Descrierea strategiei de livrare aplicate (ex: "Livrare Express — 1-2 zile, +100 MDL").</summary>
          public string DeliveryStrategyDescription { get; set; } = "";

          /// <summary>Numarul estimat de zile lucratoare pana la livrare.</summary>
          public int EstimatedDeliveryDays { get; set; }
     }
}
