using CH_Store.Application.Admin.Interfaces;
using CH_Store.Application.DBContext;
using CH_Store.Domain.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Admin.Services
{
     /// <summary>
     /// Furnizeaza statisticile pentru dashboard-ul adminului.
     ///
     /// GetStatsAsync          — snapshot complet: comenzi, venituri, produse, utilizatori
     /// GetRevenueLastDaysAsync — venituri grupate pe zile (ultimele N zile)
     /// GetRecentActivityAsync  — ultimele N evenimente din OrderEventLogs
     /// GetOrdersByStatusAsync  — distributia comenzilor pe statusuri
     /// </summary>
     public class AdminDashboardService : IAdminDashboardService
     {
          private readonly AppDbContext _db;

          public AdminDashboardService(AppDbContext db) => _db = db;

          // ── Stats principale ──────────────────────────────────────────────────

          public async Task<AdminDashboardStats> GetStatsAsync()
          {
               var today     = DateTime.UtcNow.Date;
               var monthStart = new DateTime(today.Year, today.Month, 1);

               // ─── Comenzi ──────────────────────────────────────────────────────
               var orderStats = await _db.Orders
                    .GroupBy(o => 1)
                    .Select(g => new
                    {
                         Total          = g.Count(),
                         TotalRevenue   = g.Sum(o => o.TotalAmount),
                         PendingApproval = g.Count(o => o.Status == "PendingApproval"),
                         InProgress      = g.Count(o => o.Status == "Processing" || o.Status == "Shipped"),
                         Cancelled       = g.Count(o => o.Status == "Cancelled")
                    })
                    .FirstOrDefaultAsync();

               var ordersToday = await _db.Orders
                    .CountAsync(o => o.OrderDate >= today);

               var deliveredToday = await _db.Orders
                    .CountAsync(o => o.Status == "Delivered" && o.OrderDate >= today);

               var revenueToday = await _db.Orders
                    .Where(o => o.OrderDate >= today && o.Status != "Cancelled")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

               var revenueThisMonth = await _db.Orders
                    .Where(o => o.OrderDate >= monthStart && o.Status != "Cancelled")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

               // ─── Produse ──────────────────────────────────────────────────────
               var productStats = await _db.Products
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                         Total      = g.Count(),
                         OutOfStock = g.Count(p => p.StockQuantity == 0),
                         LowStock   = g.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 10)
                    })
                    .FirstOrDefaultAsync();

               // ─── Utilizatori ──────────────────────────────────────────────────
               var totalUsers = await _db.Users.CountAsync();
               var newUsersThisMonth = await _db.Users
                    .CountAsync(u => u.RegistartionDateTime >= monthStart);

               // ─── Sub-liste ────────────────────────────────────────────────────
               var ordersByStatus  = await GetOrdersByStatusAsync();
               var revenueLastDays = await GetRevenueLastDaysAsync(7);
               var stockAlerts     = await GetStockAlertsAsync();
               var recentActivity  = await GetRecentActivityAsync(10);

               return new AdminDashboardStats
               {
                    // Comenzi
                    TotalOrders       = orderStats?.Total           ?? 0,
                    OrdersToday       = ordersToday,
                    PendingApproval   = orderStats?.PendingApproval ?? 0,
                    InProgress        = orderStats?.InProgress      ?? 0,
                    DeliveredToday    = deliveredToday,
                    CancelledTotal    = orderStats?.Cancelled       ?? 0,

                    // Venituri
                    TotalRevenue      = orderStats?.TotalRevenue    ?? 0,
                    RevenueToday      = revenueToday,
                    RevenueThisMonth  = revenueThisMonth,

                    // Produse
                    TotalProducts      = productStats?.Total      ?? 0,
                    OutOfStockProducts = productStats?.OutOfStock ?? 0,
                    LowStockProducts   = productStats?.LowStock   ?? 0,

                    // Utilizatori
                    TotalUsers        = totalUsers,
                    NewUsersThisMonth = newUsersThisMonth,

                    // Liste
                    OrdersByStatus  = ordersByStatus,
                    RevenueLastDays = revenueLastDays,
                    StockAlerts     = stockAlerts,
                    RecentActivity  = recentActivity
               };
          }

          // ── Venituri pe zile ──────────────────────────────────────────────────

          public async Task<List<RevenueByPeriodDto>> GetRevenueLastDaysAsync(int days = 30)
          {
               var cutoff = DateTime.UtcNow.Date.AddDays(-days + 1);

               var raw = await _db.Orders
                    .Where(o => o.OrderDate >= cutoff && o.Status != "Cancelled")
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                         Date       = g.Key,
                         Revenue    = g.Sum(o => o.TotalAmount),
                         OrderCount = g.Count()
                    })
                    .OrderBy(g => g.Date)
                    .ToListAsync();

               // Umple zilele fara comenzi cu 0
               var result = new List<RevenueByPeriodDto>();
               for (int i = 0; i < days; i++)
               {
                    var date  = cutoff.AddDays(i);
                    var entry = raw.FirstOrDefault(r => r.Date == date);
                    result.Add(new RevenueByPeriodDto
                    {
                         Period     = date.ToString("yyyy-MM-dd"),
                         Revenue    = entry?.Revenue    ?? 0,
                         OrderCount = entry?.OrderCount ?? 0
                    });
               }

               return result;
          }

          // ── Activitate recenta (OrderEventLogs) ───────────────────────────────

          public async Task<List<RecentActivityEntry>> GetRecentActivityAsync(int count = 50)
               => await _db.OrderEventLogs
                    .OrderByDescending(e => e.OccurredAt)
                    .Take(count)
                    .Select(e => new RecentActivityEntry
                    {
                         OrderId    = e.OrderId,
                         UserId     = e.UserId,
                         OldStatus  = e.OldStatus,
                         NewStatus  = e.NewStatus,
                         Total      = e.OrderTotal,
                         OccurredAt = e.OccurredAt,
                         Notes      = e.Notes
                    })
                    .ToListAsync();

          // ── Comenzi pe status ─────────────────────────────────────────────────

          public async Task<List<OrdersByStatusEntry>> GetOrdersByStatusAsync()
               => await _db.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new OrdersByStatusEntry
                    {
                         Status     = g.Key,
                         Count      = g.Count(),
                         TotalValue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(e => e.Count)
                    .ToListAsync();

          // ── Helper privat ─────────────────────────────────────────────────────

          private async Task<List<LowStockAlertDto>> GetStockAlertsAsync(int threshold = 10)
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
     }
}
