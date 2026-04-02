using CH_Store.Application.Product.Interfaces;
using CH_Store.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Proxy
{
     public class ProductRemoteProxy : IProductRepo
     {
          private readonly IProductRepo _realService;

          // Cache-ul este static pentru a persista între diferite cereri HTTP (Scoped)
          private static readonly ConcurrentDictionary<int, ProductPrototypeData> _productCache = new();
          private static List<ProductPrototypeData>? _allProductsCache;

          public ProductRemoteProxy(IProductRepo realService)
          {
               _realService = realService;
          }

          public async Task<ProductPrototypeData?> GetByIdAsync(int id)
          {
               // Verificăm dacă îl avem în cache
               if (_productCache.TryGetValue(id, out var cachedProduct))
               {
                    Console.WriteLine($"[Proxy Cache] Hit! Returnat din memorie: {cachedProduct.Name}");
                    return cachedProduct;
               }

               // Dacă nu e în cache, mergem la baza de date
               var product = await _realService.GetByIdAsync(id);

               if (product != null)
               {
                    _productCache.TryAdd(id, product);
               }

               return product;
          }

          public async Task<IEnumerable<ProductPrototypeData>> GetAllAsync()
          {
               if (_allProductsCache != null)
               {
                    Console.WriteLine("[Proxy Cache] Returnare listă completă din memorie.");
                    return _allProductsCache;
               }

               var products = await _realService.GetAllAsync();
               _allProductsCache = products.ToList();

               return _allProductsCache;
          }
     }
}
