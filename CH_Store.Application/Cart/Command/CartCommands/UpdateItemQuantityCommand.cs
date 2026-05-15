using CH_Store.Domain.Models;

namespace CH_Store.Application.Cart.Command.CartCommands
{
     /// <summary>
     /// Concrete Command — actualizeaza cantitatea unui produs din cos.
     ///
     /// Execute:
     ///   • NewQuantity > 0 → seteaza cantitatea noua
     ///   • NewQuantity = 0 → sterge produsul din cos (echivalent cu RemoveItem)
     ///
     /// Undo: restaureaza cantitatea anterioara (sau readauga produsul daca era 0).
     /// </summary>
     public class UpdateItemQuantityCommand : ICommand
     {
          private readonly CartData _cart;
          private readonly int      _productId;
          private readonly int      _newQuantity;

          // Snapshot pentru Undo
          private int       _previousQuantity;
          private CartItem? _removedItem;    // snapshot daca produsul a fost sters (qty=0)

          public UpdateItemQuantityCommand(CartData cart, int productId, int newQuantity)
          {
               _cart        = cart;
               _productId   = productId;
               _newQuantity = newQuantity;
          }

          public string   CommandName => nameof(UpdateItemQuantityCommand);
          public string   Description => _newQuantity > 0
               ? $"Actualizat cantitate produs ID {_productId}: → {_newQuantity}"
               : $"Eliminat produs ID {_productId} din cos (cantitate 0)";
          public DateTime OccurredAt  { get; } = DateTime.UtcNow;

          public Task ExecuteAsync()
          {
               var existing = _cart.Items.FirstOrDefault(i => i.ProductId == _productId);

               if (existing == null)
                    return Task.CompletedTask;

               _previousQuantity = existing.Quantity;

               if (_newQuantity <= 0)
               {
                    _removedItem = existing.Clone();
                    _cart.Items.Remove(existing);
               }
               else
               {
                    existing.Quantity = _newQuantity;
               }

               _cart.UpdatedAt = DateTime.UtcNow;
               return Task.CompletedTask;
          }

          public Task UndoAsync()
          {
               if (_removedItem != null)
               {
                    // Produsul a fost sters — readaugam cu cantitatea originala
                    _removedItem.Quantity = _previousQuantity;
                    _cart.Items.Add(_removedItem);
               }
               else
               {
                    // Restauram cantitatea anterioara
                    var existing = _cart.Items.FirstOrDefault(i => i.ProductId == _productId);
                    if (existing != null)
                         existing.Quantity = _previousQuantity;
               }

               _cart.UpdatedAt = DateTime.UtcNow;
               return Task.CompletedTask;
          }
     }
}
