using CH_Store.Application.Admin.Interfaces;
using CH_Store.Application.DBContext;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.DTOs.Admin;
using CH_Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Admin.Services
{
     /// <summary>
     /// Gestioneaza utilizatorii pentru admin.
     ///
     /// GetAllAsync   — lista paginata cu cautare dupa Username/Email
     /// GetByIdAsync  — detalii user + statistici comenzi
     /// UpdateAsync   — actualizare partiala: Email, Phone, Address, Level
     /// GetUserOrdersAsync — toate comenzile unui user
     /// </summary>
     public class AdminUserService : IAdminUserService
     {
          private readonly AppDbContext _db;

          public AdminUserService(AppDbContext db) => _db = db;

          // ── Read ──────────────────────────────────────────────────────────────

          public async Task<PagedResult<AdminUserSummary>> GetAllAsync(
               int page = 1, int pageSize = 20, string? search = null)
          {
               var query = _db.Users.AsQueryable();

               if (!string.IsNullOrWhiteSpace(search))
               {
                    string term = search.ToLower();
                    query = query.Where(u =>
                         u.Username.ToLower().Contains(term) ||
                         u.Email.ToLower().Contains(term));
               }

               int total = await query.CountAsync();

               // Subquery: numar comenzi + total cheltuit per user
               var userIds = await query
                    .OrderBy(u => u.Username)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => u.Id)
                    .ToListAsync();

               var users = await query
                    .Where(u => userIds.Contains(u.Id))
                    .OrderBy(u => u.Username)
                    .ToListAsync();

               // Statistici comenzi per user (un singur query)
               var orderStats = await _db.Orders
                    .Where(o => userIds.Contains(o.UserId))
                    .GroupBy(o => o.UserId)
                    .Select(g => new
                    {
                         UserId     = g.Key,
                         Count      = g.Count(),
                         TotalSpent = g.Sum(o => o.TotalAmount)
                    })
                    .ToListAsync();

               var statsMap = orderStats.ToDictionary(s => s.UserId);

               var items = users.Select(u =>
               {
                    statsMap.TryGetValue(u.Id, out var stats);
                    return new AdminUserSummary
                    {
                         Id                   = u.Id,
                         Username             = u.Username,
                         Email                = u.Email,
                         Phone                = u.Phone,
                         Address              = u.Address,
                         Level                = u.Level,
                         LastLoginDateTime    = u.LastLoginDateTime,
                         RegistartionDateTime = u.RegistartionDateTime,
                         TotalOrders          = stats?.Count      ?? 0,
                         TotalSpent           = stats?.TotalSpent ?? 0
                    };
               }).ToList();

               return new PagedResult<AdminUserSummary>
               {
                    Items      = items,
                    TotalCount = total,
                    Page       = page,
                    PageSize   = pageSize
               };
          }

          public async Task<AdminUserSummary?> GetByIdAsync(int userId)
          {
               var user = await _db.Users.FindAsync(userId);
               if (user == null) return null;

               var stats = await _db.Orders
                    .Where(o => o.UserId == userId)
                    .GroupBy(o => o.UserId)
                    .Select(g => new { Count = g.Count(), TotalSpent = g.Sum(o => o.TotalAmount) })
                    .FirstOrDefaultAsync();

               return new AdminUserSummary
               {
                    Id                   = user.Id,
                    Username             = user.Username,
                    Email                = user.Email,
                    Phone                = user.Phone,
                    Address              = user.Address,
                    Level                = user.Level,
                    LastLoginDateTime    = user.LastLoginDateTime,
                    RegistartionDateTime = user.RegistartionDateTime,
                    TotalOrders          = stats?.Count      ?? 0,
                    TotalSpent           = stats?.TotalSpent ?? 0
               };
          }

          // ── Update ────────────────────────────────────────────────────────────

          public async Task<AdminUserSummary?> UpdateAsync(int userId, AdminUserUpdateRequest req)
          {
               var user = await _db.Users.FindAsync(userId);
               if (user == null) return null;

               if (req.Email   != null) user.Email   = req.Email;
               if (req.Phone   != null) user.Phone   = req.Phone;
               if (req.Address != null) user.Address = req.Address;
               if (req.Level   != null) user.Level   = req.Level.Value;

               await _db.SaveChangesAsync();

               return await GetByIdAsync(userId);
          }

          // ── Comenzi user ──────────────────────────────────────────────────────

          public async Task<IEnumerable<OrderDbTable>> GetUserOrdersAsync(int userId)
               => await _db.Orders
                    .Include(o => o.Items)
                         .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
     }
}
