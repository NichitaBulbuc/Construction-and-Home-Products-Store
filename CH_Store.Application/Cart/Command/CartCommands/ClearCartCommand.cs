using CH_Store.Domain.Models;

namespace CH_Store.Application.Cart.Command.CartCommands
{
     /// <summary>
     /// Concrete Command — goleste complet cosul de cumparaturi.
     ///
     /// Execute: salveaza un snapshot complet al tuturor produselor, apoi le sterge.
     /// Undo:    restaureaza toate produsele din snapshot.
     ///
     /// Acesta este exemplul clasic unde Command + snapshot permite
     /// "undo" al unei operatii destructive masive.
     /// </summary>
     public class ClearCartCommand : ICommand
     {
          private readonly CartData _cart;

          // Snapshot complet al cosului inainte de stergere
          private List<CartItem> _snapshot = new();

          public ClearCartCommand(CartData cart) => _cart = cart;

          public string   CommandName => nameof(ClearCartCommand);
          public string   Description => $"Golit cosul ({_cart.LineCount} produse, {_cart.Total:F2} MDL)";
          public DateTime OccurredAt  { get; } = DateTime.UtcNow;

          public Task ExecuteAsync()
          {
               // Snapshot profund — clonam fiecare item inainte de stergere
               _snapshot = _cart.Items.Select(i => i.Clone()).ToList();

               _cart.Items.Clear();
               _cart.UpdatedAt = DateTime.UtcNow;

               return Task.CompletedTask;
          }

          public Task UndoAsync()
          {
               // Restauram toate produsele din snapshot
               _cart.Items.Clear();
               _cart.Items.AddRange(_snapshot);
               _cart.UpdatedAt = DateTime.UtcNow;

               return Task.CompletedTask;
          }
     }
}
