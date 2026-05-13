using CH_Store.Application.Product.Interfaces;
using CH_Store.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace CH_Store.Application.Product.Proxy
{
     /// <summary>
     /// Remote Proxy Pattern — Proxy (Cache Proxy).
     ///
     /// Intercepteaza apelurile catre RealSubject (ProductDbService / SQL Server)
     /// si le raspunde din cache (IMemoryCache) cand datele sunt disponibile.
     ///
     /// ┌──────────────┐        ┌───────────────────────┐        ┌──────────────────┐
     /// │   Client     │───────▶│  ProductRemoteProxy   │───────▶│ ProductDbService │
     /// │(Controller)  │        │  (Proxy / Cache)      │  miss  │  (RealSubject)   │
     /// └──────────────┘        │                       │        │   SQL Server     │
     ///                         │  IMemoryCache         │        └──────────────────┘
     ///                         │  TTL: 5 minute        │
     ///                         │  Invalidare: CToken   │
     ///                         └───────────────────────┘
     ///
     /// Probleme rezolvate fata de versiunea precedenta:
     ///   ✗ static ConcurrentDictionary → fara TTL, niciodata invalidat
     ///   ✓ IMemoryCache (Singleton) → TTL configurabil, thread-safe, gestionat de ASP.NET Core
     ///
     ///   ✗ Cache partajat periculos intre instante Scoped via static
     ///   ✓ IMemoryCache este Singleton injectat — lifecycle corect, partajare intentionata
     ///
     ///   ✗ Nicio posibilitate de invalidare controlata
     ///   ✓ static CancellationTokenSource → invalidare de grup cross-request via token
     ///
     ///   ✗ RealSubject era ProductService (InMemory ProductContext)
     ///   ✓ RealSubject este ProductDbService (SQL Server AppDbContext)
     /// </summary>
     public class ProductRemoteProxy : IProductRepo
     {
          private readonly IProductRepo  _realService;
          private readonly IMemoryCache  _cache;

          // ─── TTL implicit ──────────────────────────────────────────────────────
          private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

          // ─── Chei de cache ─────────────────────────────────────────────────────
          private static string KeyById(int id)             => $"product:id:{id}";
          private static string KeyAll()                    => "product:all";
          private static string KeyByCategory(string cat)  => $"product:cat:{cat.ToLower().Trim()}";

          // ─── Token global pentru invalidare de grup (cross-request) ───────────
          // static: toate instantele Proxy (per-request Scoped) partajeaza acelasi token.
          // Cand este anulat, TOATE intrarile de cache inregistrate cu el sunt evictate
          // simultan, indiferent de request-ul care le-a creat.
          private static CancellationTokenSource _globalEvictionCts = new();

          public ProductRemoteProxy(IProductRepo realService, IMemoryCache cache)
          {
               _realService = realService;
               _cache       = cache;
          }

          // ════════════════════════════════════════════════════════════════════
          // IProductRepo — implementare cu cache
          // ════════════════════════════════════════════════════════════════════

          public async Task<ProductDbTable?> GetByIdAsync(int id)
          {
               string key = KeyById(id);

               if (_cache.TryGetValue(key, out ProductDbTable? cached))
               {
                    Console.WriteLine($"[Proxy HIT]  product:id:{id} → returnat din IMemoryCache");
                    return cached;
               }

               Console.WriteLine($"[Proxy MISS] product:id:{id} → delegat catre RealSubject (SQL Server)");

               var product = await _realService.GetByIdAsync(id);

               if (product != null)
                    _cache.Set(key, product, BuildOptions());

               return product;
          }

          public async Task<IEnumerable<ProductDbTable>> GetAllAsync()
          {
               string key = KeyAll();

               if (_cache.TryGetValue(key, out IEnumerable<ProductDbTable>? cached))
               {
                    Console.WriteLine("[Proxy HIT]  product:all → returnat din IMemoryCache");
                    return cached!;
               }

               Console.WriteLine("[Proxy MISS] product:all → delegat catre RealSubject (SQL Server)");

               var products = await _realService.GetAllAsync();
               var list     = products.ToList();

               _cache.Set(key, (IEnumerable<ProductDbTable>)list, BuildOptions());

               return list;
          }

          public async Task<IEnumerable<ProductDbTable>> GetByCategoryAsync(string category)
          {
               string key = KeyByCategory(category);

               if (_cache.TryGetValue(key, out IEnumerable<ProductDbTable>? cached))
               {
                    Console.WriteLine($"[Proxy HIT]  product:cat:{category.ToLower()} → returnat din IMemoryCache");
                    return cached!;
               }

               Console.WriteLine($"[Proxy MISS] product:cat:{category.ToLower()} → delegat catre RealSubject (SQL Server)");

               var products = await _realService.GetByCategoryAsync(category);
               var list     = products.ToList();

               _cache.Set(key, (IEnumerable<ProductDbTable>)list, BuildOptions());

               return list;
          }

          // ════════════════════════════════════════════════════════════════════
          // Invalidare cache
          // ════════════════════════════════════════════════════════════════════

          /// <summary>
          /// Invalideaza cache-ul pentru un produs specific.
          /// Elimina si product:all (devine stale cand un produs se modifica).
          /// </summary>
          public void InvalidateProduct(int productId)
          {
               _cache.Remove(KeyById(productId));
               _cache.Remove(KeyAll());
               Console.WriteLine($"[Proxy] Cache invalidat: product:id:{productId} + product:all");
          }

          /// <summary>
          /// Invalideaza TOT cache-ul de produse — toate request-urile sunt afectate.
          /// Mecanism: anuleaza CancellationTokenSource-ul global → toate intrarile
          /// inregistrate cu acest token sunt evictate simultan din IMemoryCache.
          /// Un nou CTS este creat imediat pentru intrarile viitoare.
          /// </summary>
          public static void InvalidateAll()
          {
               var old = Interlocked.Exchange(ref _globalEvictionCts, new CancellationTokenSource());
               old.Cancel();
               old.Dispose();
               Console.WriteLine("[Proxy] Cache global invalidat — toate intrarile de produse evictate.");
          }

          // ════════════════════════════════════════════════════════════════════
          // Helper privat
          // ════════════════════════════════════════════════════════════════════

          /// <summary>
          /// Construieste optiunile de cache:
          ///   • TTL absolut de 5 minute (intrarea expira indiferent de accese)
          ///   • Token de evictie global — InvalidateAll() le evicteaza pe toate simultan
          /// </summary>
          private static MemoryCacheEntryOptions BuildOptions() =>
               new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(DefaultTtl)
                    .AddExpirationToken(new CancellationChangeToken(_globalEvictionCts.Token));
     }
}
