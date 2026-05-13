namespace CH_Store.Domain.Models
{
     /// <summary>
     /// Reprezinta un produs dintr-o comanda (in-memory).
     /// </summary>
     public class OrderItemData
     {
          public int ProductId { get; set; }

          public string ProductName { get; set; } = "";

          public double Price { get; set; }

          public int Quantity { get; set; }

          public double Subtotal => Price * Quantity;
     }
}
