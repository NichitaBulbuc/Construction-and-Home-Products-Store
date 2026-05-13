using CH_Store.Application.DBContext;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.DbRepo
{
     public class OrderRepo : IOrderRepo
     {
          private readonly AppDbContext _db;

          public OrderRepo(AppDbContext db)
          {
               _db = db;
          }

          /// <inheritdoc/>
          public async Task<int> SaveAsync(OrderData order)
          {
               // 1. Mapam OrderData -> OrderDbTable
               var entity = new OrderDbTable
               {
                    UserId        = order.UserId,
                    OrderDate     = order.CreatedAt,
                    TotalAmount   = order.TotalPrice,
                    Status        = order.Status,
                    ShippingAddress = order.DeliveryAddress,
                    DeliveryType  = order.DeliveryType.ToString(),
                    DeliveryCost  = order.DeliveryCost,
                    HasInstallation = order.HasInstallation,
                    IsPriority    = order.IsPriority,
                    Discount      = order.Discount,
                    Notes         = string.IsNullOrWhiteSpace(order.Notes) ? null : order.Notes,
                    ReportSnapshot = string.IsNullOrWhiteSpace(order.ReportSnapshot) ? null : order.ReportSnapshot
               };

               _db.Orders.Add(entity);
               await _db.SaveChangesAsync(); // genereaza entity.Id

               // 2. Mapam fiecare OrderItemData -> OrderItemDbTable
               foreach (var item in order.Items)
               {
                    _db.OrderItems.Add(new OrderItemDbTable
                    {
                         OrderId     = entity.Id,
                         ProductId   = item.ProductId,
                         ProductName = item.ProductName,
                         UnitPrice   = (decimal)item.Price,
                         Quantity    = item.Quantity,
                         Subtotal    = (decimal)item.Subtotal
                    });
               }

               await _db.SaveChangesAsync();
               return entity.Id;
          }

          /// <inheritdoc/>
          public async Task<OrderDbTable?> GetByIdAsync(int id)
          {
               return await _db.Orders
                    .Include(o => o.Items)
                         .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);
          }

          /// <inheritdoc/>
          public async Task<IEnumerable<OrderDbTable>> GetAllAsync()
          {
               return await _db.Orders
                    .Include(o => o.Items)
                         .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
          }

          /// <inheritdoc/>
          public async Task<IEnumerable<OrderDbTable>> GetByUserIdAsync(int userId)
          {
               return await _db.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.Items)
                         .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
          }

          /// <inheritdoc/>
          public async Task<bool> UpdateStatusAsync(int orderId, string newStatus)
          {
               var entity = await _db.Orders.FindAsync(orderId);

               if (entity == null)
                    return false;

               entity.Status = newStatus;
               await _db.SaveChangesAsync();
               return true;
          }

          /// <inheritdoc/>
          public async Task<IEnumerable<OrderEventLogDbTable>> GetOrderEventsAsync(int orderId)
          {
               return await _db.OrderEventLogs
                    .Where(e => e.OrderId == orderId)
                    .OrderByDescending(e => e.OccurredAt)
                    .ToListAsync();
          }

          /// <inheritdoc/>
          public async Task<IEnumerable<OrderEventLogDbTable>> GetAllEventsAsync()
          {
               return await _db.OrderEventLogs
                    .OrderByDescending(e => e.OccurredAt)
                    .ToListAsync();
          }
     }
}
