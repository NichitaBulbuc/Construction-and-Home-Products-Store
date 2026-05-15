using CH_Store.Domain.Enums;

namespace CH_Store.Domain.DTOs
{
     public class OrderRequest
     {
          public int UserId { get; set; }

          public List<OrderItemRequest> Items { get; set; } = new();

          public string DeliveryAddress { get; set; } = "";

          public DeliveryType DeliveryType { get; set; } = DeliveryType.Standard;

          public bool WantsInstallation { get; set; }

          public bool IsPriority { get; set; }

          public decimal Discount { get; set; }

          public string Notes { get; set; } = "";
     }
}
