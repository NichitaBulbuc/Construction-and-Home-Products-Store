namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// Intrare in jurnalul de comenzi (Command Pattern audit log).
     /// Stocata in IMemoryCache per userId, returnata de GET /api/cart/{userId}/history.
     /// </summary>
     public class CommandHistoryEntry
     {
          public int    Index       { get; set; }
          public string CommandName { get; set; } = "";
          public string Description { get; set; } = "";
          public DateTime OccurredAt { get; set; }
          public bool   WasUndone   { get; set; }

          public string Status => WasUndone ? "Anulat (Undo)" : "Executat";
     }
}
