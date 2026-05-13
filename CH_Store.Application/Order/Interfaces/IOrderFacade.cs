using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Interfaces
{
     public interface IOrderFacade
     {
          /// <summary>Construieste si salveaza o comanda standard (produse + livrare).</summary>
          Task<(OrderData Order, string Report, int DbId)> PlaceStandardOrderAsync(OrderRequest dto);

          /// <summary>Construieste si salveaza o comanda completa cu toate optiunile.</summary>
          Task<(OrderData Order, string Report, int DbId)> PlaceFullOrderAsync(OrderRequest dto);

          /// <summary>Comanda cu livrare Express + prioritate fortata.</summary>
          Task<(OrderData Order, string Report, int DbId)> PlaceExpressOrderAsync(OrderRequest dto);

          /// <summary>Comanda en-gros cu reducere aplicata automat.</summary>
          Task<(OrderData Order, string Report, int DbId)> PlaceBulkOrderAsync(OrderRequest dto);

          /// <summary>Returneaza o comanda din DB dupa ID.</summary>
          Task<OrderDbTable?> GetOrderAsync(int id);

          /// <summary>Returneaza toate comenzile unui user.</summary>
          Task<IEnumerable<OrderDbTable>> GetOrdersByUserAsync(int userId);
     }
}
