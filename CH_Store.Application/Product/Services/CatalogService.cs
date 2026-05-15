using CH_Store.Application.DBContext;
using CH_Store.Application.Product.Catalog;
using CH_Store.Application.Product.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Product.Services
{
     /// <summary>
     /// Implementarea serviciului de catalog — orchestreaza Composite Pattern cu SQL Server.
     ///
     /// Fluxul principal:
     ///   1. Incarca date din AppDbContext (ProductDbTable, CatalogKitDbTable, CatalogKitItemDbTable)
     ///   2. Construieste arborele Composite: ProductKit (Composite) + IndividualProduct (Leaf)
     ///   3. Apeleaza ToCatalogNodeDto() pe radacina arborelui pentru raspuns API
     ///
     /// Detectia ciclurilor: HashSet<int> visited previne bucle infinite la imbricarea recursiva.
     /// </summary>
     public class CatalogService : ICatalogService
     {
          private readonly AppDbContext _db;

          private const int MaxTreeDepth = 10;

          public CatalogService(AppDbContext db)
          {
               _db = db;
          }

          // ════════════════════════════════════════════════════════════════════
          // CITIRE — Constructia arborelui Composite din DB
          // ════════════════════════════════════════════════════════════════════

          /// <summary>
          /// Catalogul complet:
          ///   • Toate produsele din ProductDbTable ca IndividualProduct (Leaf)
          ///   • Toate kiturile active ca ProductKit (Composite) cu arborele lor complet
          /// </summary>
          public async Task<IEnumerable<CatalogNodeDto>> GetFullCatalogAsync()
          {
               var result = new List<CatalogNodeDto>();

               // ── Produse individuale (Leaf-uri) ─────────────────────────────────
               var products = await _db.Products
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

               foreach (var p in products)
                    result.Add(new IndividualProduct(p).ToCatalogNodeDto());

               // ── Kituri (Composite-uri) ─────────────────────────────────────────
               var kitIds = await _db.CatalogKits
                    .Where(k => k.IsActive)
                    .Select(k => k.Id)
                    .ToListAsync();

               foreach (int kitId in kitIds)
               {
                    var treeDto = await GetKitTreeAsync(kitId);
                    if (treeDto != null)
                         result.Add(treeDto);
               }

               return result;
          }

          /// <summary>
          /// Construieste arborele complet al unui kit din DB.
          /// Rezolvare recursiva cu detectie cicluri prin HashSet.
          /// </summary>
          public async Task<CatalogNodeDto?> GetKitTreeAsync(int kitId)
          {
               var visited = new HashSet<int>();
               var root    = await BuildKitTreeAsync(kitId, visited, depth: 0);
               return root?.ToCatalogNodeDto();
          }

          /// <summary>Returneaza lista plata a tuturor kiturilor (fara arborele lor complet).</summary>
          public async Task<IEnumerable<CatalogKitDbTable>> GetAllKitsAsync()
          {
               return await _db.CatalogKits
                    .Where(k => k.IsActive)
                    .OrderBy(k => k.Name)
                    .ToListAsync();
          }

          // ════════════════════════════════════════════════════════════════════
          // SCRIERE — Persistenta structurii Composite
          // ════════════════════════════════════════════════════════════════════

          /// <summary>Creeaza un kit nou gol in DB. Returneaza ID-ul generat.</summary>
          public async Task<int> CreateKitAsync(CreateKitRequest request)
          {
               var kit = new CatalogKitDbTable
               {
                    Name        = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    IsActive    = true,
                    CreatedAt   = DateTime.UtcNow
               };

               _db.CatalogKits.Add(kit);
               await _db.SaveChangesAsync();

               return kit.Id;
          }

          /// <summary>
          /// Adauga un produs (Leaf) la kit.
          /// Verifica existenta kitului si a produsului inainte de salvare.
          /// </summary>
          public async Task<bool> AddProductToKitAsync(int kitId, AddProductToKitRequest request)
          {
               bool kitExists     = await _db.CatalogKits.AnyAsync(k => k.Id == kitId && k.IsActive);
               bool productExists = await _db.Products.AnyAsync(p => p.Id == request.ProductId);

               if (!kitExists || !productExists)
                    return false;

               var item = new CatalogKitItemDbTable
               {
                    KitId     = kitId,
                    ProductId = request.ProductId,
                    SubKitId  = null,
                    Quantity  = request.Quantity,
                    SortOrder = request.SortOrder
               };

               _db.CatalogKitItems.Add(item);
               await _db.SaveChangesAsync();
               return true;
          }

          /// <summary>
          /// Adauga un sub-kit (Composite copil) la un kit parinte.
          /// Previne ciclurile verificand daca sub-kitul contine deja (direct sau indirect)
          /// kitul parinte in arborele sau.
          /// </summary>
          public async Task<bool> AddSubKitToKitAsync(int kitId, AddSubKitRequest request)
          {
               int subKitId = request.SubKitId;

               if (kitId == subKitId) return false; // auto-referinta

               bool parentExists = await _db.CatalogKits.AnyAsync(k => k.Id == kitId  && k.IsActive);
               bool childExists  = await _db.CatalogKits.AnyAsync(k => k.Id == subKitId && k.IsActive);

               if (!parentExists || !childExists) return false;

               // Verificare ciclu: sub-kitul nu trebuie sa aiba kitId ca descendent
               bool wouldCreateCycle = await IsDescendantAsync(subKitId, kitId);
               if (wouldCreateCycle) return false;

               var item = new CatalogKitItemDbTable
               {
                    KitId     = kitId,
                    ProductId = null,
                    SubKitId  = subKitId,
                    Quantity  = 1,
                    SortOrder = request.SortOrder
               };

               _db.CatalogKitItems.Add(item);
               await _db.SaveChangesAsync();
               return true;
          }

          /// <summary>Sterge un element dintr-un kit (nu sterge produsul/sub-kitul referentiat).</summary>
          public async Task<bool> RemoveKitItemAsync(int kitItemId)
          {
               var item = await _db.CatalogKitItems.FindAsync(kitItemId);
               if (item == null) return false;

               _db.CatalogKitItems.Remove(item);
               await _db.SaveChangesAsync();
               return true;
          }

          /// <summary>Sterge un kit si toate elementele sale (cascade in DB).</summary>
          public async Task<bool> DeleteKitAsync(int kitId)
          {
               var kit = await _db.CatalogKits.FindAsync(kitId);
               if (kit == null) return false;

               // Soft delete — pastram istoricul, kitul nu mai apare in catalog
               kit.IsActive = false;
               await _db.SaveChangesAsync();
               return true;
          }

          // ════════════════════════════════════════════════════════════════════
          // PRIVATE — Algoritmul recursiv de build arbore
          // ════════════════════════════════════════════════════════════════════

          /// <summary>
          /// Incarca recursiv un kit din DB si construieste nodul ProductKit (Composite).
          ///
          /// Algoritm:
          ///   1. Daca kitId e in visited sau depth > MaxTreeDepth → opreste recursivitatea
          ///   2. Incarca kitul cu Items + Products din DB (un singur query cu Include)
          ///   3. Creeaza ProductKit cu datele kitului
          ///   4. Pentru fiecare item, in ordinea SortOrder:
          ///      a. Item cu ProductId  → new IndividualProduct(item.Product, item.Quantity) — Leaf
          ///      b. Item cu SubKitId   → recursie BuildKitTreeAsync(subKitId)              — Composite
          ///   5. Adauga toti copiii la ProductKit si returneaza
          /// </summary>
          private async Task<ProductKit?> BuildKitTreeAsync(
               int kitId, HashSet<int> visited, int depth)
          {
               if (depth > MaxTreeDepth || visited.Contains(kitId))
                    return null;

               visited.Add(kitId);

               var kit = await _db.CatalogKits
                    .Include(k => k.Items)
                         .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(k => k.Id == kitId && k.IsActive);

               if (kit == null)
                    return null;

               var compositeNode = new ProductKit(kit.Id, kit.Name, kit.Description);

               foreach (var item in kit.Items.OrderBy(i => i.SortOrder))
               {
                    if (item.ProductId.HasValue && item.Product != null)
                    {
                         // Leaf: produs individual
                         compositeNode.Add(new IndividualProduct(item.Product, item.Quantity));
                    }
                    else if (item.SubKitId.HasValue)
                    {
                         // Composite: sub-kit — recursie cu depth + 1
                         var subKit = await BuildKitTreeAsync(item.SubKitId.Value, visited, depth + 1);
                         if (subKit != null)
                              compositeNode.Add(subKit);
                    }
               }

               return compositeNode;
          }

          /// <summary>
          /// Verifica daca <paramref name="ancestorId"/> este descendent al lui <paramref name="rootKitId"/>.
          /// Folosit pentru detectia ciclurilor inainte de AddSubKitToKitAsync.
          /// </summary>
          private async Task<bool> IsDescendantAsync(int rootKitId, int ancestorId)
          {
               var visited = new HashSet<int>();
               var queue   = new Queue<int>();
               queue.Enqueue(rootKitId);

               while (queue.Count > 0)
               {
                    int current = queue.Dequeue();

                    if (current == ancestorId) return true;
                    if (visited.Contains(current)) continue;

                    visited.Add(current);

                    var childKitIds = await _db.CatalogKitItems
                         .Where(i => i.KitId == current && i.SubKitId.HasValue)
                         .Select(i => i.SubKitId!.Value)
                         .ToListAsync();

                    foreach (int childId in childKitIds)
                         queue.Enqueue(childId);
               }

               return false;
          }
     }
}
