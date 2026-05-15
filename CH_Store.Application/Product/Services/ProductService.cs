using CH_Store.Application.DBContext;
using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Product.Services
{
     /// <summary>
     /// Serviciu pentru Prototype Pattern — lucreaza cu ProductContext (InMemory).
     ///
     /// NU implementeaza IProductRepo — aceasta interfata este acum rezervata
     /// pentru lantul Remote Proxy Pattern (SQL Server):
     ///   IProductRepo → ProductRemoteProxy (Proxy) → ProductDbService (RealSubject)
     ///
     /// ProductService ramane separat: citeste/scrie ProductPrototypeData din InMemory DB,
     /// folosit exclusiv pentru crearea de prototipuri via create-from-prototype endpoint.
     /// </summary>
     public class ProductService
     {
          private readonly ProductContext _context;

          public ProductService(ProductContext context)
          {
               _context = context;
          }

          public async Task<ProductPrototypeData?> GetByIdAsync(int id)
          {
               Console.WriteLine($"[Prototype DB] InMemory → SELECT * FROM Products WHERE Id = {id}");
               return await _context.Products.FindAsync(id);
          }

          public async Task<IEnumerable<ProductPrototypeData>> GetAllAsync()
          {
               Console.WriteLine("[Prototype DB] InMemory → SELECT * FROM Products");
               return await _context.Products.ToListAsync();
          }
     }
}
