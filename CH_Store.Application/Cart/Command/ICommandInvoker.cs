using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Cart.Command
{
     /// <summary>
     /// Invoker — primeste comenzi, le executa si le pastreaza in stiva de undo.
     /// Stiva + istoricul sunt persistate in IMemoryCache per userId (TTL 4h sliding).
     /// </summary>
     public interface ICommandInvoker
     {
          /// <summary>Executa comanda si o adauga in stiva de undo si in jurnal.</summary>
          Task ExecuteAsync(ICommand command, int userId);

          /// <summary>
          /// Anuleaza ultima comanda din stiva.
          /// Returneaza (true, mesaj) sau (false, "Nu exista operatii de anulat").
          /// </summary>
          Task<(bool Success, string Message)> UndoLastAsync(int userId);

          /// <summary>Numarul de comenzi disponibile pentru undo.</summary>
          int GetUndoStackDepth(int userId);

          /// <summary>Jurnalul complet al comenzilor executate de user.</summary>
          IReadOnlyList<CommandHistoryEntry> GetHistory(int userId);

          /// <summary>Sterge stiva si istoricul pentru userId (ex: la logout sau checkout).</summary>
          void ClearHistory(int userId);
     }
}
