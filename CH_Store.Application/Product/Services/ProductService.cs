using CH_Store.Application.DBContext;
using CH_Store.Application.Product.Interfaces;
using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Services
{
     public class ProductService : IProductRepo
     {
          private readonly ProductContext _context;

          public ProductService(ProductContext context)
          {
               _context = context;
          }

          public async Task<ProductPrototypeData?> GetByIdAsync(int id)
          {
               Console.WriteLine($"[Database] Accesare SQL pentru produsul cu ID: {id}");
               return await _context.Products.FindAsync(id);
          }

          public async Task<IEnumerable<ProductPrototypeData>> GetAllAsync()
          {
               Console.WriteLine("[Database] Preluare listă completă din SQL Server");
               return await _context.Products.ToListAsync();
          }
     }
}
