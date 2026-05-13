using CH_Store.Domain.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace CH_Store.Application.Cart.Command
{
     /// <summary>
     /// Invoker concret — pastreaza stiva de undo si jurnalul per userId in IMemoryCache.
     ///
     /// Design:
     ///   • IMemoryCache stocheaza referinte la obiectele ICommand (nu serializate) →
     ///     comenzile cart pot modifica CartData in-place la undo, chiar cross-request.
     ///   • TTL sliding 4h — daca userul nu interactioneaza, stiva expira automat.
     ///   • Inregistrat ca Singleton (IMemoryCache este Singleton) — thread-safe
     ///     prin faptul ca modificarile CartData sunt secventiale per user.
     /// </summary>
     public class CommandInvoker : ICommandInvoker
     {
          private readonly IMemoryCache _cache;
          private static readonly TimeSpan Ttl = TimeSpan.FromHours(4);

          // Chei IMemoryCache
          private static string StackKey(int userId)   => $"cmd-stack:{userId}";
          private static string HistoryKey(int userId) => $"cmd-history:{userId}";

          public CommandInvoker(IMemoryCache cache) => _cache = cache;

          // ────────────────────────────────────────────────────────────────────
          public async Task ExecuteAsync(ICommand command, int userId)
          {
               await command.ExecuteAsync();

               // Adauga in stiva de undo
               var stack = GetStack(userId);
               stack.Push(command);
               SetStack(userId, stack);

               // Adauga in jurnal
               var history = GetHistory_Internal(userId);
               history.Add(new CommandHistoryEntry
               {
                    Index       = history.Count + 1,
                    CommandName = command.CommandName,
                    Description = command.Description,
                    OccurredAt  = command.OccurredAt,
                    WasUndone   = false
               });
               SetHistory(userId, history);
          }

          // ────────────────────────────────────────────────────────────────────
          public async Task<(bool Success, string Message)> UndoLastAsync(int userId)
          {
               var stack = GetStack(userId);

               if (stack.Count == 0)
                    return (false, "Nu exista operatii de anulat pentru acest utilizator.");

               var command = stack.Pop();
               await command.UndoAsync();
               SetStack(userId, stack);

               // Marcheaza ultima intrare nehidden din jurnal ca anulata
               var history = GetHistory_Internal(userId);
               var entry   = history.LastOrDefault(h => !h.WasUndone && h.CommandName == command.CommandName);
               if (entry != null) entry.WasUndone = true;
               SetHistory(userId, history);

               return (true, $"Operatia \"{command.Description}\" a fost anulata cu succes.");
          }

          // ────────────────────────────────────────────────────────────────────
          public int GetUndoStackDepth(int userId) => GetStack(userId).Count;

          public IReadOnlyList<CommandHistoryEntry> GetHistory(int userId)
               => GetHistory_Internal(userId).AsReadOnly();

          public void ClearHistory(int userId)
          {
               _cache.Remove(StackKey(userId));
               _cache.Remove(HistoryKey(userId));
          }

          // ── Helpers ─────────────────────────────────────────────────────────

          private Stack<ICommand> GetStack(int userId)
               => _cache.GetOrCreate(StackKey(userId), e =>
               {
                    e.SlidingExpiration = Ttl;
                    return new Stack<ICommand>();
               })!;

          private void SetStack(int userId, Stack<ICommand> stack)
               => _cache.Set(StackKey(userId), stack, SlidingOptions());

          private List<CommandHistoryEntry> GetHistory_Internal(int userId)
               => _cache.GetOrCreate(HistoryKey(userId), e =>
               {
                    e.SlidingExpiration = Ttl;
                    return new List<CommandHistoryEntry>();
               })!;

          private void SetHistory(int userId, List<CommandHistoryEntry> history)
               => _cache.Set(HistoryKey(userId), history, SlidingOptions());

          private static MemoryCacheEntryOptions SlidingOptions()
               => new() { SlidingExpiration = Ttl };
     }
}
