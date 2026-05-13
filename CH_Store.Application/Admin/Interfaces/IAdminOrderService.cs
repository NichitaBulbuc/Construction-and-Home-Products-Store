using CH_Store.Domain.DTOs;
using CH_Store.Domain.DTOs.Admin;
using CH_Store.Domain.Entities;

namespace CH_Store.Application.Admin.Interfaces
{
     public interface IAdminOrderService
     {
          Task<PagedResult<AdminOrderSummary>>  GetAllAsync(AdminOrderFilterRequest filter);
          Task<OrderDbTable?>                   GetDetailAsync(int orderId);
          Task<IEnumerable<AdminOrderSummary>>  GetPendingApprovalAsync();

          Task<OrderOperationResult>            ApproveOrderAsync(int orderId, AdminApproveOrderRequest request);
          Task<OrderOperationResult>            RejectOrderAsync(int orderId, AdminRejectOrderRequest request);
          Task<OrderOperationResult>            ForceUpdateStatusAsync(int orderId, OrderStatusRequest request);
     }
}
