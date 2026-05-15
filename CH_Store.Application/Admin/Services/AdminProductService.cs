using CH_Store.Application.Admin.Interfaces;
using CH_Store.Application.DBContext;
using CH_Store.Application.Product.Proxy;
using CH_Store.Domain.DTOs.Admin;
using CH_Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Admin.Services
{
     /// <summary>
     /// Gestioneaza produsele din SQL Server (AppDbContext) pentru admin.
     /// Dupa orice mutatie (Create/Update/Delete/Stock) invalideaza
     /// ProductRemoteProxy cache — urmatoarele GET-uri vor incarca date proaspete.
     /// </summary>
     public class AdminProductService : IAdminProductService
     {
          private readonly AppDbContext _db;

          public AdminProductService(AppDbContext db) => _db = db;

          // ── Read ──────────────────────────────────────────────────────────────

          public async Task<IEnumerable<ProductDbTable>> GetAllAsync(string? category = null)
          {
               var query = _db.Products.AsQueryable();

               if (!string.IsNullOrWhiteSpace(category))
                    query = query.Where(p => p.Category.ToLower() == category.ToLower());

               return await query.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
          }

          public async Task<ProductDbTable?> GetByIdAsync(int id)
               => await _db.Products.FindAsync(id);

          public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int threshold = 10)
               => await _db.Products
                    .Where(p => p.StockQuantity <= threshold)
                    .OrderBy(p => p.StockQuantity)
                    .Select(p => new LowStockAlertDto
                    {
                         ProductId     = p.Id,
                         ProductName   = p.Name,
                         Category      = p.Category,
                         StockQuantity = p.StockQuantity,
                         Price         = p.Price
                    })
                    .ToListAsync();

          // ── Create ────────────────────────────────────────────────────────────

          public async Task<ProductDbTable> CreateAsync(CreateProductRequest req)
          {
               var product = new ProductDbTable
               {
                    Name          = req.Name,
                    Description   = req.Description,
                    Price         = req.Price,
                    StockQuantity = req.StockQuantity,
                    Category      = req.Category,
                    Weight        = req.Weight,
                    EnergyClass   = req.EnergyClass,
                    CreatedDateTime = DateTime.Now
               };

               _db.Products.Add(product);
               await _db.SaveChangesAsync();

               ProductRemoteProxy.InvalidateAll();
               return product;
          }

          // ── Update ────────────────────────────────────────────────────────────

          public async Task<ProductDbTable?> UpdateAsync(int id, UpdateProductRequest req)
          {
               var product = await _db.Products.FindAsync(id);
               if (product == null) return null;

               if (req.Name        != null) product.Name        = req.Name;
               if (req.Description != null) product.Description = req.Description;
               if (req.Price       != null) product.Price       = req.Price.Value;
               if (req.Category    != null) product.Category    = req.Category;
               if (req.Weight      != null) product.Weight      = req.Weight;
               if (req.EnergyClass != null) product.EnergyClass = req.EnergyClass;

               await _db.SaveChangesAsync();

               ProductRemoteProxy.InvalidateAll();
               return product;
          }

          public async Task<ProductDbTable?> UpdateStockAsync(int id, UpdateStockRequest req)
          {
               var product = await _db.Products.FindAsync(id);
               if (product == null) return null;

               product.StockQuantity = req.StockQuantity;
               await _db.SaveChangesAsync();

               ProductRemoteProxy.InvalidateAll();
               return product;
          }

          // ── Delete ────────────────────────────────────────────────────────────

          public async Task<bool> DeleteAsync(int id)
          {
               var product = await _db.Products.FindAsync(id);
               if (product == null) return false;

               // Verifica daca produsul este referentiat in comenzi active
               bool hasActiveOrders = await _db.OrderItems
                    .AnyAsync(oi => oi.ProductId == id &&
                                   oi.Order.Status != "Cancelled" &&
                                   oi.Order.Status != "Delivered");

               if (hasActiveOrders)
                    throw new InvalidOperationException(
                         $"Produsul '{product.Name}' nu poate fi sters — " +
                         "exista comenzi active care il contin.");

               _db.Products.Remove(product);
               await _db.SaveChangesAsync();

               ProductRemoteProxy.InvalidateAll();
               return true;
          }
     }
}
