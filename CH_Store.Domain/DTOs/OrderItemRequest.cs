namespace CH_Store.Domain.DTOs
{
     public class OrderItemRequest
     {
          /// <summary>ID-ul produsului din ProductDbTable.</summary>
          public int ProductId { get; set; }

          public string ProductName { get; set; } = "";

          public double Price { get; set; }

          public int Quantity { get; set; }
     }
}
