using CH_Store.Domain.Models;

namespace CH_Store.Domain.DTOs
{
     /// <summary>Raspunsul complet al cosului de cumparaturi.</summary>
     public class CartResponse
     {
          public int    UserId    { get; set; }
          public string SessionId { get; set; } = "";

          public List<CartItem> Items { get; set; } = new();

          public decimal Total     { get; set; }
          public int     ItemCount { get; set; }
          public int     LineCount { get; set; }
          public bool    IsEmpty   { get; set; }

          public DateTime UpdatedAt { get; set; }

          // ── Command metadata ─────────────────────────────────────────────────
          /// <summary>Numarul de operatii disponibile pentru Undo.</summary>
          public int  UndoStackDepth  { get; set; }
          public bool CanUndo         => UndoStackDepth > 0;

          /// <summary>Mesajul operatiei tocmai executate (pentru feedback imediat).</summary>
          public string? LastOperation { get; set; }

          public static CartResponse From(CartData cart, int undoDepth, string? lastOp = null) => new()
          {
               UserId         = cart.UserId,
               SessionId      = cart.SessionId,
               Items          = cart.Items,
               Total          = cart.Total,
               ItemCount      = cart.ItemCount,
               LineCount      = cart.LineCount,
               IsEmpty        = cart.IsEmpty,
               UpdatedAt      = cart.UpdatedAt,
               UndoStackDepth = undoDepth,
               LastOperation  = lastOp
          };
     }
}
