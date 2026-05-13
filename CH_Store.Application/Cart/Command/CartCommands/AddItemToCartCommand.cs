using CH_Store.Domain.Models;

namespace CH_Store.Application.Cart.Command.CartCommands
{
     /// <summary>
     /// Concrete Command — adauga un produs in cosul de cumparaturi.
     ///
     /// Execute:
     ///   • Daca produsul exista deja → incrementeaza cantitatea
     ///   • Daca nu exista          → adauga linie noua
     ///
     /// Undo:
     ///   • Daca produsul era deja in cos → restaureaza cantitatea anterioara
     ///   • Daca era produs nou          → sterge linia din cos
     /// </summary>
     public class AddItemToCartCommand : ICommand
     {
          private readonly CartData _cart;
          private readonly CartItem _item;

          // Snapshot pentru Undo
          private bool _wasExistingLine;
          private int  _previousQuantity;

          public AddItemToCartCommand(CartData cart, CartItem item)
          {
               _cart = cart;
               _item = item;
          }

          public string   CommandName => nameof(AddItemToCartCommand);
          public string   Description => $"Adaugat '{_item.ProductName}' x{_item.Quantity} in cos";
          public DateTime OccurredAt  { get; } = DateTime.UtcNow;

          public Task ExecuteAsync()
          {
               var existing = _cart.Items.FirstOrDefault(i => i.ProductId == _item.ProductId);

               if (existing != null)
               {
                    _wasExistingLine  = true;
                    _previousQuantity = existing.Quantity;
                    existing.Quantity += _item.Quantity;
               }
               else
               {
                    _wasExistingLine = false;
                    _cart.Items.Add(_item);
               }

               _cart.UpdatedAt = DateTime.UtcNow;
               return Task.CompletedTask;
          }

          public Task UndoAsync()
          {
               if (_wasExistingLine)
               {
                    // Restaureaza cantitatea anterioara
                    var existing = _cart.Items.FirstOrDefault(i => i.ProductId == _item.ProductId);
                    if (existing != null)
                         existing.Quantity = _previousQuantity;
               }
               else
               {
                    // Sterge produsul adaugat
                    _cart.Items.RemoveAll(i => i.ProductId == _item.ProductId);
               }

               _cart.UpdatedAt = DateTime.UtcNow;
               return Task.CompletedTask;
          }
     }
}
