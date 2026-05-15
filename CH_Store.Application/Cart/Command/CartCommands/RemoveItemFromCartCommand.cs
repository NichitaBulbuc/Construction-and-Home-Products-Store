using CH_Store.Domain.Models;

namespace CH_Store.Application.Cart.Command.CartCommands
{
     /// <summary>
     /// Concrete Command — sterge un produs din cosul de cumparaturi.
     ///
     /// Execute: elimina linia cu productId din cos.
     /// Undo:    restaureaza linia stearsa (cu cantitatea si pretul originale).
     /// </summary>
     public class RemoveItemFromCartCommand : ICommand
     {
          private readonly CartData _cart;
          private readonly int      _productId;

          // Snapshot pentru Undo
          private CartItem? _removedItem;
          private int       _removedAtIndex;

          public RemoveItemFromCartCommand(CartData cart, int productId)
          {
               _cart      = cart;
               _productId = productId;
          }

          public string   CommandName => nameof(RemoveItemFromCartCommand);
          public string   Description => $"Eliminat produsul ID {_productId} din cos";
          public DateTime OccurredAt  { get; } = DateTime.UtcNow;

          public Task ExecuteAsync()
          {
               _removedItem = _cart.Items.FirstOrDefault(i => i.ProductId == _productId);

               if (_removedItem == null)
                    return Task.CompletedTask;   // nimic de sters

               _removedAtIndex = _cart.Items.IndexOf(_removedItem);
               _removedItem    = _removedItem.Clone();   // snapshot inainte de stergere
               _cart.Items.RemoveAll(i => i.ProductId == _productId);
               _cart.UpdatedAt = DateTime.UtcNow;

               return Task.CompletedTask;
          }

          public Task UndoAsync()
          {
               if (_removedItem == null)
                    return Task.CompletedTask;   // nu a existat, nimic de restaurat

               // Reinserteaza la pozitia originala (sau la final daca indexul a depasit lista)
               int insertAt = Math.Min(_removedAtIndex, _cart.Items.Count);
               _cart.Items.Insert(insertAt, _removedItem);
               _cart.UpdatedAt = DateTime.UtcNow;

               return Task.CompletedTask;
          }
     }
}
