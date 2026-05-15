using CH_Store.Application.Admin.Interfaces;
using CH_Store.Application.DBContext;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Observer;
using CH_Store.Application.Order.Strategy.Payment;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.DTOs.Admin;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Admin.Services
{
     /// <summary>
     /// Gestioneaza comenzile pentru admin.
     ///
     /// ApproveOrderAsync:
     ///   • Status: PendingApproval → Processing (sau PaymentFailed daca plata esueaza)
     ///   • Opțional: initiaza plata prin IPaymentStrategyResolver (Strategy Pattern)
     ///   • Notificare automata prin IOrderEventPublisher (Observer Pattern)
     ///
     /// RejectOrderAsync:
     ///   • Status: orice → Cancelled
     ///   • Motivul este salvat in Notes + eveniment Observer
     /// </summary>
     public class AdminOrderService : IAdminOrderService
     {
          private readonly AppDbContext            _db;
          private readonly IOrderEventPublisher    _eventPublisher;
          private readonly IPaymentStrategyResolver _paymentStrategyResolver;

          public AdminOrderService(
               AppDbContext             db,
               IOrderEventPublisher     eventPublisher,
               IPaymentStrategyResolver paymentStrategyResolver)
          {
               _db                      = db;
               _eventPublisher          = eventPublisher;
               _paymentStrategyResolver = paymentStrategyResolver;
          }

          // ── Read ──────────────────────────────────────────────────────────────

          public async Task<PagedResult<AdminOrderSummary>> GetAllAsync(AdminOrderFilterRequest filter)
          {
               var query = _db.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .AsQueryable();

               // Filtre
               if (!string.IsNullOrWhiteSpace(filter.Status))
                    query = query.Where(o => o.Status == filter.Status);

               if (filter.UserId.HasValue)
                    query = query.Where(o => o.UserId == filter.UserId.Value);

               if (filter.DateFrom.HasValue)
                    query = query.Where(o => o.OrderDate >= filter.DateFrom.Value);

               if (filter.DateTo.HasValue)
                    query = query.Where(o => o.OrderDate <= filter.DateTo.Value.AddDays(1));

               if (filter.MinTotal.HasValue)
                    query = query.Where(o => o.TotalAmount >= filter.MinTotal.Value);

               if (filter.MaxTotal.HasValue)
                    query = query.Where(o => o.TotalAmount <= filter.MaxTotal.Value);

               // Sortare
               query = filter.SortBy switch
               {
                    "TotalAmount" => filter.SortDesc ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                    "Status"      => filter.SortDesc ? query.OrderByDescending(o => o.Status)      : query.OrderBy(o => o.Status),
                    "UserId"      => filter.SortDesc ? query.OrderByDescending(o => o.UserId)      : query.OrderBy(o => o.UserId),
                    _             => filter.SortDesc ? query.OrderByDescending(o => o.OrderDate)   : query.OrderBy(o => o.OrderDate)
               };

               int total = await query.CountAsync();

               var items = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(o => new AdminOrderSummary
                    {
                         Id              = o.Id,
                         UserId          = o.UserId,
                         Username        = o.User != null ? o.User.Username : null,
                         OrderDate       = o.OrderDate,
                         TotalAmount     = o.TotalAmount,
                         Status          = o.Status,
                         DeliveryType    = o.DeliveryType,
                         ItemCount       = o.Items.Count,
                         HasInstallation = o.HasInstallation,
                         IsPriority      = o.IsPriority,
                         Discount        = o.Discount,
                         Notes           = o.Notes
                    })
                    .ToListAsync();

               return new PagedResult<AdminOrderSummary>
               {
                    Items      = items,
                    TotalCount = total,
                    Page       = filter.Page,
                    PageSize   = filter.PageSize
               };
          }

          public async Task<OrderDbTable?> GetDetailAsync(int orderId)
               => await _db.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                         .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

          public async Task<IEnumerable<AdminOrderSummary>> GetPendingApprovalAsync()
               => await _db.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .Where(o => o.Status == "PendingApproval")
                    .OrderBy(o => o.OrderDate)
                    .Select(o => new AdminOrderSummary
                    {
                         Id              = o.Id,
                         UserId          = o.UserId,
                         Username        = o.User != null ? o.User.Username : null,
                         OrderDate       = o.OrderDate,
                         TotalAmount     = o.TotalAmount,
                         Status          = o.Status,
                         DeliveryType    = o.DeliveryType,
                         ItemCount       = o.Items.Count,
                         HasInstallation = o.HasInstallation,
                         IsPriority      = o.IsPriority,
                         Discount        = o.Discount,
                         Notes           = o.Notes
                    })
                    .ToListAsync();

          // ── Aprobare ──────────────────────────────────────────────────────────

          public async Task<OrderOperationResult> ApproveOrderAsync(int orderId, AdminApproveOrderRequest req)
          {
               var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
               if (order == null)
                    return Fail(orderId, $"Comanda #{orderId} nu a fost gasita.");

               if (order.Status != "PendingApproval")
                    return Fail(orderId,
                         $"Comanda #{orderId} are statusul '{order.Status}' — " +
                         "doar comenzile 'PendingApproval' pot fi aprobate.");

               string oldStatus = order.Status;
               string newStatus = "Processing";
               string paymentInfo = "";

               // Opțional: procesare plata la aprobare
               if (req.ProcessPaymentWith.HasValue)
               {
                    try
                    {
                         var strategy = _paymentStrategyResolver.Resolve(req.ProcessPaymentWith.Value);
                         var result   = strategy.Execute(order.TotalAmount);

                         newStatus   = result.Success ? "Processing" : "PaymentFailed";
                         paymentInfo = result.Success
                              ? $" | Plata procesata via {req.ProcessPaymentWith}: {result.TransactionId}"
                              : $" | Plata esuata: {result.Message}";
                    }
                    catch (Exception ex)
                    {
                         paymentInfo = $" | Eroare plata: {ex.Message}";
                    }
               }

               order.Status = newStatus;
               if (!string.IsNullOrWhiteSpace(req.Notes))
                    order.Notes = (order.Notes + $"\n[Admin] {req.Notes}").Trim();

               await _db.SaveChangesAsync();

               // Observer: notifica client, actualizeaza stoc, audit log
               await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
               {
                    OrderId             = orderId,
                    UserId              = order.UserId,
                    OldStatus           = oldStatus,
                    NewStatus           = newStatus,
                    OrderTotal          = order.TotalAmount,
                    Items               = order.Items
                         .Select(i => new OrderEventItem(i.ProductId, i.ProductName, i.Quantity)).ToList(),
                    RecipientContact    = req.RecipientContact ?? "",
                    RecipientName       = req.RecipientName ?? "",
                    NotificationChannel = req.NotificationChannel
               });

               return new OrderOperationResult
               {
                    Success   = true,
                    OrderId   = orderId,
                    NewStatus = newStatus,
                    Message   = $"Comanda #{orderId} aprobata. Status: {oldStatus} → {newStatus}.{paymentInfo}",
                    NotificationSent = !string.IsNullOrWhiteSpace(req.RecipientContact)
               };
          }

          // ── Respingere ────────────────────────────────────────────────────────

          public async Task<OrderOperationResult> RejectOrderAsync(int orderId, AdminRejectOrderRequest req)
          {
               var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
               if (order == null)
                    return Fail(orderId, $"Comanda #{orderId} nu a fost gasita.");

               if (order.Status is "Cancelled" or "Delivered")
                    return Fail(orderId,
                         $"Comanda #{orderId} este deja '{order.Status}' — nu poate fi respinsa.");

               string oldStatus = order.Status;
               order.Status = "Cancelled";
               order.Notes  = (order.Notes + $"\n[Admin Reject] {req.Reason}").Trim();

               await _db.SaveChangesAsync();

               await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
               {
                    OrderId             = orderId,
                    UserId              = order.UserId,
                    OldStatus           = oldStatus,
                    NewStatus           = "Cancelled",
                    OrderTotal          = order.TotalAmount,
                    Items               = order.Items
                         .Select(i => new OrderEventItem(i.ProductId, i.ProductName, i.Quantity)).ToList(),
                    RecipientContact    = req.RecipientContact ?? "",
                    RecipientName       = req.RecipientName ?? "",
                    NotificationChannel = req.NotificationChannel
               });

               return new OrderOperationResult
               {
                    Success   = true,
                    OrderId   = orderId,
                    NewStatus = "Cancelled",
                    Message   = $"Comanda #{orderId} respinsa. Motiv: {req.Reason}",
                    NotificationSent = !string.IsNullOrWhiteSpace(req.RecipientContact)
               };
          }

          // ── Force status ──────────────────────────────────────────────────────

          public async Task<OrderOperationResult> ForceUpdateStatusAsync(int orderId, OrderStatusRequest request)
          {
               if (string.IsNullOrWhiteSpace(request.NewStatus))
                    return Fail(orderId, "Statusul nou nu poate fi gol.");

               var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
               if (order == null)
                    return Fail(orderId, $"Comanda #{orderId} nu a fost gasita.");

               string oldStatus = order.Status;
               order.Status = request.NewStatus;
               await _db.SaveChangesAsync();

               await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
               {
                    OrderId             = orderId,
                    UserId              = order.UserId,
                    OldStatus           = oldStatus,
                    NewStatus           = request.NewStatus,
                    OrderTotal          = order.TotalAmount,
                    Items               = order.Items
                         .Select(i => new OrderEventItem(i.ProductId, i.ProductName, i.Quantity)).ToList(),
                    RecipientContact    = request.RecipientContact ?? "",
                    RecipientName       = request.RecipientName ?? "",
                    NotificationChannel = request.NotificationChannel ?? "email"
               });

               return new OrderOperationResult
               {
                    Success   = true,
                    OrderId   = orderId,
                    NewStatus = request.NewStatus,
                    Message   = $"Status comanda #{orderId}: {oldStatus} → {request.NewStatus}.",
                    NotificationSent = !string.IsNullOrWhiteSpace(request.RecipientContact)
               };
          }

          private static OrderOperationResult Fail(int id, string msg)
               => new() { Success = false, OrderId = id, Message = msg };
     }
}
