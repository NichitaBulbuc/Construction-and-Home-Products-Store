using CH_Store.Domain.DTOs.Admin;

namespace CH_Store.Application.Admin.Interfaces
{
     public interface IAdminDashboardService
     {
          Task<AdminDashboardStats>          GetStatsAsync();
          Task<List<RevenueByPeriodDto>>     GetRevenueLastDaysAsync(int days = 30);
          Task<List<RecentActivityEntry>>    GetRecentActivityAsync(int count = 50);
          Task<List<OrdersByStatusEntry>>    GetOrdersByStatusAsync();
     }
}
