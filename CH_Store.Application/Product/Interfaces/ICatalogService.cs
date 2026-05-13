using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;

namespace CH_Store.Application.Product.Interfaces
{
     /// <summary>
     /// Serviciu pentru catalogul Composite Pattern.
     ///
     /// Responsabilitati:
     ///   1. Incarca datele din AppDbContext (SQL Server)
     ///   2. Construieste arborele Composite (ProductKit + IndividualProduct)
     ///   3. Persista structura kiturilor in CatalogKitDbTable / CatalogKitItemDbTable
     ///   4. Returneaza DTO-uri structurate pentru API
     /// </summary>
     public interface ICatalogService
     {
          /// <summary>
          /// Returneaza catalogul complet:
          ///   • Toate produsele individuale din ProductDbTable (fiecare ca Leaf)
          ///   • Toate kiturile active cu arborele lor complet (fiecare ca Composite)
          /// </summary>
          Task<IEnumerable<CatalogNodeDto>> GetFullCatalogAsync();

          /// <summary>
          /// Construieste si returneaza arborele complet al unui kit din DB.
          /// Rezolva recursiv sub-kiturile. Detecteaza cicluri (max 10 niveluri).
          /// Returneaza null daca kitul nu exista.
          /// </summary>
          Task<CatalogNodeDto?> GetKitTreeAsync(int kitId);

          /// <summary>Returneaza lista tuturor kiturilor (fara arbore complet).</summary>
          Task<IEnumerable<CatalogKitDbTable>> GetAllKitsAsync();

          /// <summary>
          /// Creeaza un kit nou gol in DB.
          /// Returneaza ID-ul generat.
          /// </summary>
          Task<int> CreateKitAsync(CreateKitRequest request);

          /// <summary>
          /// Adauga un produs (Leaf) la un kit existent.
          /// Returneaza false daca kitul sau produsul nu exista.
          /// </summary>
          Task<bool> AddProductToKitAsync(int kitId, AddProductToKitRequest request);

          /// <summary>
          /// Adauga un sub-kit (Composite copil) la un kit parinte.
          /// Previne ciclurile: nu permite adaugarea unui kit ca sub-kit al sau propriu
          /// sau al unui descendent al sau.
          /// Returneaza false daca oricare dintre kituri nu exista sau ar crea ciclu.
          /// </summary>
          Task<bool> AddSubKitToKitAsync(int kitId, AddSubKitRequest request);

          /// <summary>
          /// Sterge un element (CatalogKitItemDbTable) dintr-un kit.
          /// Nu sterge produsul sau sub-kitul referentiat.
          /// Returneaza false daca elementul nu exista.
          /// </summary>
          Task<bool> RemoveKitItemAsync(int kitItemId);

          /// <summary>
          /// Sterge un kit si toate elementele sale (cascade).
          /// Nu sterge produsele sau sub-kiturile referentiate.
          /// Returneaza false daca kitul nu exista.
          /// </summary>
          Task<bool> DeleteKitAsync(int kitId);
     }
}
