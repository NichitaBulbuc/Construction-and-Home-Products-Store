namespace CH_Store.Domain.DTOs
{
     /// <summary>Cerere de adaugare produs in cos.</summary>
     public class CartItemRequest
     {
          public int    ProductId   { get; set; }
          public string ProductName { get; set; } = "";
          public double Price       { get; set; }
          public int    Quantity    { get; set; } = 1;
     }
}
