using CH_Store.Domain.Entities;

namespace CH_Store.Application.Product.Interfaces
{
     /// <summary>
     /// Remote Proxy Pattern — interfata uniforma (Subject).
     ///
     /// Atat RealSubject (ProductDbService) cat si Proxy (ProductRemoteProxy)
     /// implementeaza aceasta interfata. Clientul nu stie cu ce lucreaza.
     ///
     /// Toate metodele lucreaza cu ProductDbTable (entitate SQL Server, AppDbContext).
     /// </summary>
     public interface IProductRepo
     {
          /// <summary>Returneaza un produs dupa ID.</summary>
          Task<ProductDbTable?> GetByIdAsync(int id);

          /// <summary>Returneaza toate produsele din catalog.</summary>
          Task<IEnumerable<ProductDbTable>> GetAllAsync();

          /// <summary>Returneaza produsele filtrate dupa categorie ("Construction" / "Home").</summary>
          Task<IEnumerable<ProductDbTable>> GetByCategoryAsync(string category);
     }
}
