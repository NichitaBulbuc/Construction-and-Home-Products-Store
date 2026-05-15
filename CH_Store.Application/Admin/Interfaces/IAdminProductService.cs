using CH_Store.Domain.DTOs.Admin;
using CH_Store.Domain.Entities;

namespace CH_Store.Application.Admin.Interfaces
{
     public interface IAdminProductService
     {
          Task<IEnumerable<ProductDbTable>> GetAllAsync(string? category = null);
          Task<ProductDbTable?>             GetByIdAsync(int id);
          Task<ProductDbTable>              CreateAsync(CreateProductRequest request);
          Task<ProductDbTable?>             UpdateAsync(int id, UpdateProductRequest request);
          Task<bool>                        DeleteAsync(int id);
          Task<ProductDbTable?>             UpdateStockAsync(int id, UpdateStockRequest request);
          Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int threshold = 10);
     }
}
