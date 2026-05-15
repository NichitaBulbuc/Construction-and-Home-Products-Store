using CH_Store.Application.DBContext;
using CH_Store.Application.Product.Interfaces;
using CH_Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Product.Services
{
     /// <summary>
     /// Remote Proxy Pattern — RealSubject.
     ///
     /// Acceseaza direct AppDbContext (SQL Server) fara niciun cache.
     /// Este apelat de ProductRemoteProxy (Proxy) doar la cache miss.
     ///
     /// Separat de ProductService care lucreaza cu ProductContext (InMemory)
     /// pentru Prototype Pattern — cele doua contexte au scopuri diferite:
     ///   ProductDbService  → AppDbContext (SQL Server) — produse reale, persistate
     ///   ProductService    → ProductContext (InMemory) — prototipuri demo, volatile
     /// </summary>
     public class ProductDbService : IProductRepo
     {
          private readonly AppDbContext _db;

          public ProductDbService(AppDbContext db)
          {
               _db = db;
          }

          public async Task<ProductDbTable?> GetByIdAsync(int id)
          {
               Console.WriteLine($"[RealSubject] SQL Server → SELECT * FROM Products WHERE Id = {id}");
               return await _db.Products.FindAsync(id);
          }

          public async Task<IEnumerable<ProductDbTable>> GetAllAsync()
          {
               Console.WriteLine("[RealSubject] SQL Server → SELECT * FROM Products ORDER BY Category, Name");
               return await _db.Products
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Name)
                    .ToListAsync();
          }

          public async Task<IEnumerable<ProductDbTable>> GetByCategoryAsync(string category)
          {
               Console.WriteLine($"[RealSubject] SQL Server → SELECT * FROM Products WHERE Category = '{category}'");
               return await _db.Products
                    .Where(p => p.Category == category)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
          }
     }
}
