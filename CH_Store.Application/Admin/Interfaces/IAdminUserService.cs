using CH_Store.Domain.DTOs;
using CH_Store.Domain.DTOs.Admin;
using CH_Store.Domain.Entities;

namespace CH_Store.Application.Admin.Interfaces
{
     public interface IAdminUserService
     {
          Task<PagedResult<AdminUserSummary>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null);
          Task<AdminUserSummary?>             GetByIdAsync(int userId);
          Task<AdminUserSummary?>             UpdateAsync(int userId, AdminUserUpdateRequest request);
          Task<IEnumerable<OrderDbTable>>     GetUserOrdersAsync(int userId);
     }
}
