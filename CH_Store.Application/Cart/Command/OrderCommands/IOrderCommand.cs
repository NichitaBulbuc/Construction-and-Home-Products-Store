using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;

namespace CH_Store.Application.Cart.Command.OrderCommands
{
     /// <summary>
     /// Interfata pentru comenzile de operatii pe Orders.
     ///
     /// Diferenta fata de ICommand (cart):
     ///   • Primeste IOrderFacade la Execute/Undo — nu stocheaza referinte Scoped
     ///   • Rezultatul este OrderOperationResult, nu void
     ///   • Gestionat de OrderCommandService (Scoped), nu de CommandInvoker (Singleton)
     /// </summary>
     public interface IOrderCommand
     {
          string   CommandName { get; }
          string   Description { get; }
          DateTime OccurredAt  { get; }
          bool     CanUndo     { get; }

          Task<OrderOperationResult> ExecuteAsync(IOrderFacade facade);
          Task<OrderOperationResult> UndoAsync(IOrderFacade facade);
     }
}
