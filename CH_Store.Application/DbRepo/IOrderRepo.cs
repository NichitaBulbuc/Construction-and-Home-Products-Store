using CH_Store.Domain.Entities;
using CH_Store.Domain.Models;

namespace CH_Store.Application.DbRepo
{
     /// <summary>
     /// Responsabilitate unica: citire/scriere comenzi in baza de date.
     /// Nu contine logica de business — doar persistenta.
     /// </summary>
     public interface IOrderRepo
     {
          /// <summary>Salveaza comanda construita de OrderBuilder si returneaza ID-ul generat de DB.</summary>
          Task<int> SaveAsync(OrderData order);

          /// <summary>Returneaza comanda cu toate produsele incluse (eager loading).</summary>
          Task<OrderDbTable?> GetByIdAsync(int id);

          /// <summary>Returneaza toate comenzile cu produsele lor.</summary>
          Task<IEnumerable<OrderDbTable>> GetAllAsync();

          /// <summary>Returneaza toate comenzile unui user specific.</summary>
          Task<IEnumerable<OrderDbTable>> GetByUserIdAsync(int userId);

          /// <summary>
          /// Actualizeaza statusul unei comenzi existente.
          /// Returneaza false daca comanda nu a fost gasita.
          /// </summary>
          Task<bool> UpdateStatusAsync(int orderId, string newStatus);
     }
}
