using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;

namespace CH_Store.Application.Order.Interfaces
{
     public interface IOrderTemplateService
     {
          /// <summary>
          /// Cloneaza o comanda existenta ca template:
          ///   1. Preia template-ul din DB
          ///   2. Aplica Prototype Pattern (deep copy + ajustari cantitati)
          ///   3. Refresheaza preturile din ProductDbTable (SQL Server)
          ///   4. Genereaza devizul comparativ
          ///   5. Salveaza noua comanda in DB
          /// </summary>
          Task<CloneOrderResponse> CloneAsync(int templateOrderId, CloneOrderRequest request);

          /// <summary>Returneaza toate comenzile care pot fi folosite ca template.</summary>
          Task<IEnumerable<OrderDbTable>> GetAllTemplatesAsync();
     }
}
