namespace CH_Store.Domain.Models
{
     /// <summary>Un produs din cosul de cumparaturi (model in-memory).</summary>
     public class CartItem
     {
          public int    ProductId   { get; set; }
          public string ProductName { get; set; } = "";
          public double Price       { get; set; }
          public int    Quantity    { get; set; }

          public decimal Subtotal => (decimal)(Price * Quantity);

          /// <summary>Clone superficial — necesar in ClearCartCommand pentru snapshot undo.</summary>
          public CartItem Clone() => new()
          {
               ProductId   = ProductId,
               ProductName = ProductName,
               Price       = Price,
               Quantity    = Quantity
          };
     }
}
