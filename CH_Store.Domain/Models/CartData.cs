namespace CH_Store.Domain.Models
{
     /// <summary>
     /// Cosul de cumparaturi al unui user — stocat in IMemoryCache (TTL 4h).
     /// Nu este entitate EF; persistenta se face prin cache in-process.
     ///
     /// Modificat exclusiv prin Command Pattern:
     ///   AddItemToCartCommand, RemoveItemFromCartCommand,
     ///   UpdateItemQuantityCommand, ClearCartCommand
     /// </summary>
     public class CartData
     {
          public int    UserId    { get; set; }
          public string SessionId { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();

          public List<CartItem> Items { get; set; } = new();

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
          public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

          // ── Calcule ──────────────────────────────────────────────────────────
          public decimal Total      => Items.Sum(i => i.Subtotal);
          public int     ItemCount  => Items.Sum(i => i.Quantity);
          public int     LineCount  => Items.Count;
          public bool    IsEmpty    => Items.Count == 0;
     }
}
