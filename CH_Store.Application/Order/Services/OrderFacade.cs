using CH_Store.Application.DbRepo;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Observer;
using CH_Store.Application.Order.Strategy.Payment;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Events;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Facade Pattern — orchestreaza toate subsistemele.
     ///
     /// Patternuri integrate:
     ///   Builder       → OrderBuilder / OrderReportBuilder / OrderDirector
     ///   Strategy      → IPaymentStrategyResolver (plata) + DeliveryStrategyResolver (livrare)
     ///   Factory Method → PaymentProvider (apelat intern de fiecare IPaymentStrategy)
     ///   Adapter       → StripePaymentAdapter (transparent prin StripePaymentStrategy)
     ///   Observer      → IOrderEventPublisher (notificare, stoc, audit log)
     ///   Persistence   → IOrderRepo (SQL Server)
     ///
     /// Facade nu mai cunoaste PaymentProvider direct — stie doar IPaymentStrategyResolver.
     /// Adaugarea unui nou mod de plata nu necesita modificari in Facade.
     /// </summary>
     public class OrderFacade : IOrderFacade
     {
          private readonly IOrderRepo              _orderRepo;
          private readonly IPaymentStrategyResolver _paymentStrategyResolver;
          private readonly IOrderEventPublisher    _eventPublisher;

          public OrderFacade(
               IOrderRepo               orderRepo,
               IPaymentStrategyResolver paymentStrategyResolver,
               IOrderEventPublisher     eventPublisher)
          {
               _orderRepo               = orderRepo;
               _paymentStrategyResolver = paymentStrategyResolver;
               _eventPublisher          = eventPublisher;
          }

          // ════════════════════════════════════════════════════════════════════
          // PLASARE COMENZI — Builder + Strategy (Delivery) + Persistenta
          // ════════════════════════════════════════════════════════════════════

          public Task<(OrderData Order, string Report, int DbId)> PlaceStandardOrderAsync(OrderRequest dto)
               => BuildAndSaveAsync(dto, (d, r) => d.ConstructStandardOrder(r));

          public Task<(OrderData Order, string Report, int DbId)> PlaceFullOrderAsync(OrderRequest dto)
               => BuildAndSaveAsync(dto, (d, r) => d.ConstructFullOrder(r));

          public Task<(OrderData Order, string Report, int DbId)> PlaceExpressOrderAsync(OrderRequest dto)
               => BuildAndSaveAsync(dto, (d, r) => d.ConstructExpressOrder(r));

          public Task<(OrderData Order, string Report, int DbId)> PlaceBulkOrderAsync(OrderRequest dto)
               => BuildAndSaveAsync(dto, (d, r) => d.ConstructBulkOrder(r));

          // ════════════════════════════════════════════════════════════════════
          // PLASARE COMANDA CU PLATA — Builder + Strategy (Payment) + Observer
          // ════════════════════════════════════════════════════════════════════

          public async Task<OrderWithPaymentResponse> PlaceOrderWithPaymentAsync(OrderWithPaymentRequest dto)
          {
               // ── Pas 1: Strategy Pattern — rezolva strategia de plata ───────────
               // Facade nu stie de CardProcessor / StripeAdapter — stie doar strategia
               var paymentStrategy = _paymentStrategyResolver.Resolve(dto.PaymentMethod);

               // ── Pas 2 + 3: Builder + Strategy (Delivery) + Persistenta ─────────
               // BuildAndSaveAsync aplica DeliveryStrategyResolver intern in OrderBuilder
               // Strategia de plata este atasata builderului pentru raport si metadata
               var (order, report, dbId) = await BuildAndSaveAsync(
                    dto,
                    (d, r) => d.ConstructFullOrder(r),
                    paymentStrategy);

               // ── Pas 4: Strategy Pattern — executa algoritmul de plata ──────────
               bool   paymentSuccess = false;
               string paymentTxId    = "";
               string paymentMessage = "";

               try
               {
                    var paymentResult = paymentStrategy.Execute(order.TotalPrice);

                    paymentSuccess = paymentResult.Success;
                    paymentTxId   = paymentResult.TransactionId;
                    paymentMessage = paymentResult.Message;
               }
               catch (Exception ex)
               {
                    paymentMessage = $"Eroare la procesarea platii: {ex.Message}";
               }

               // ── Pas 5: Determina noul status si actualizeaza DB ────────────────
               string newStatus = paymentSuccess ? "Processing" : "PaymentFailed";
               await _orderRepo.UpdateStatusAsync(dbId, newStatus);
               order.Status = newStatus;

               // ── Pas 6: Observer Pattern — publica eveniment ───────────────────
               await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
               {
                    OrderId             = dbId,
                    UserId              = order.UserId,
                    OldStatus           = "New",
                    NewStatus           = newStatus,
                    OrderTotal          = order.TotalPrice,
                    Items               = order.Items.Select(i => new OrderEventItem(i.ProductId, i.ProductName, i.Quantity)).ToList(),
                    RecipientContact    = dto.RecipientContact,
                    RecipientName       = dto.RecipientName,
                    NotificationChannel = dto.NotificationChannel
               });

               // ── Pas 7: Asamblare raspuns complet ──────────────────────────────
               return new OrderWithPaymentResponse
               {
                    DbId            = dbId,
                    BuilderId       = order.BuilderId,
                    UserId          = order.UserId,
                    Items           = order.Items,
                    DeliveryAddress = order.DeliveryAddress,
                    DeliveryType    = order.DeliveryType,
                    DeliveryCost    = order.DeliveryCost,
                    HasInstallation = order.HasInstallation,
                    IsPriority      = order.IsPriority,
                    Discount        = order.Discount,
                    Notes           = order.Notes,
                    TotalPrice      = order.TotalPrice,
                    Status          = order.Status,
                    CreatedAt       = order.CreatedAt,
                    Report          = report,

                    // Strategy metadata
                    DeliveryStrategyDescription = order.DeliveryStrategyDescription,
                    EstimatedDeliveryDays       = order.EstimatedDeliveryDays,
                    PaymentStrategyDescription  = paymentStrategy.GetDescription(),

                    PaymentSuccess       = paymentSuccess,
                    PaymentTransactionId = paymentTxId,
                    PaymentMessage       = paymentMessage,
                    PaymentMethod        = dto.PaymentMethod.ToString(),

                    NotificationSent    = !string.IsNullOrWhiteSpace(dto.RecipientContact),
                    NotificationChannel = dto.NotificationChannel
               };
          }

          // ════════════════════════════════════════════════════════════════════
          // GESTIONARE STATUS — Persistenta + Observer
          // ════════════════════════════════════════════════════════════════════

          public async Task<OrderOperationResult> UpdateOrderStatusAsync(int orderId, OrderStatusRequest request)
          {
               if (string.IsNullOrWhiteSpace(request.NewStatus))
                    return Fail(orderId, "Statusul nou nu poate fi gol.");

               var order = await _orderRepo.GetByIdAsync(orderId);
               if (order == null)
                    return Fail(orderId, $"Comanda cu ID {orderId} nu a fost gasita.");

               string oldStatus = order.Status;
               await _orderRepo.UpdateStatusAsync(orderId, request.NewStatus);

               await _eventPublisher.PublishAsync(BuildEvent(order, oldStatus, request.NewStatus, request));

               return new OrderOperationResult
               {
                    Success          = true,
                    OrderId          = orderId,
                    NewStatus        = request.NewStatus,
                    NotificationSent = request.HasNotificationData,
                    Message          = $"Statusul comenzii #{orderId} actualizat: {oldStatus} → {request.NewStatus}."
               };
          }

          public async Task<OrderOperationResult> CancelOrderAsync(int orderId, OrderStatusRequest? notification = null)
          {
               var order = await _orderRepo.GetByIdAsync(orderId);

               if (order == null)
                    return Fail(orderId, $"Comanda cu ID {orderId} nu a fost gasita.");

               if (order.Status is "Cancelled" or "PaymentFailed")
                    return Fail(orderId, $"Comanda #{orderId} este deja anulata (status: {order.Status}).");

               if (order.Status == "Delivered")
                    return Fail(orderId, $"Comanda #{orderId} a fost livrata si nu poate fi anulata.");

               string oldStatus = order.Status;
               await _orderRepo.UpdateStatusAsync(orderId, "Cancelled");

               var req = notification ?? new OrderStatusRequest();
               await _eventPublisher.PublishAsync(BuildEvent(order, oldStatus, "Cancelled", req));

               string reason = !string.IsNullOrWhiteSpace(req.Reason) ? $" Motiv: {req.Reason}." : "";

               return new OrderOperationResult
               {
                    Success          = true,
                    OrderId          = orderId,
                    NewStatus        = "Cancelled",
                    NotificationSent = req.HasNotificationData,
                    Message          = $"Comanda #{orderId} anulata.{reason}"
               };
          }

          // ════════════════════════════════════════════════════════════════════
          // INTEROGARI
          // ════════════════════════════════════════════════════════════════════

          public Task<OrderDbTable?> GetOrderAsync(int id)
               => _orderRepo.GetByIdAsync(id);

          public Task<IEnumerable<OrderDbTable>> GetOrdersByUserAsync(int userId)
               => _orderRepo.GetByUserIdAsync(userId);

          public Task<IEnumerable<OrderEventLogDbTable>> GetOrderEventsAsync(int orderId)
               => _orderRepo.GetOrderEventsAsync(orderId);

          public Task<IEnumerable<OrderEventLogDbTable>> GetAllEventsAsync()
               => _orderRepo.GetAllEventsAsync();

          // ════════════════════════════════════════════════════════════════════
          // METODE PRIVATE
          // ════════════════════════════════════════════════════════════════════

          /// <summary>
          /// Overload fara strategie de plata (plasare standard fara payment).
          /// </summary>
          private Task<(OrderData Order, string Report, int DbId)> BuildAndSaveAsync(
               OrderRequest dto,
               Action<OrderDirector, OrderRequest> constructStrategy)
               => BuildAndSaveAsync(dto, constructStrategy, null);

          /// <summary>
          /// Construieste comanda cu Builder Pattern, aplica strategia de plata in builder
          /// (pentru raport), apoi persista in DB.
          /// </summary>
          private async Task<(OrderData Order, string Report, int DbId)> BuildAndSaveAsync(
               OrderRequest dto,
               Action<OrderDirector, OrderRequest> constructStrategy,
               IPaymentStrategy? paymentStrategy)
          {
               var orderBuilder  = new OrderBuilder();
               var orderDirector = new OrderDirector(orderBuilder);
               constructStrategy(orderDirector, dto);

               if (paymentStrategy != null)
                    orderBuilder.SetPaymentStrategy(paymentStrategy);

               var order = orderBuilder.GetResult();

               var reportBuilder  = new OrderReportBuilder();
               var reportDirector = new OrderDirector(reportBuilder);
               constructStrategy(reportDirector, dto);

               if (paymentStrategy != null)
                    reportBuilder.SetPaymentStrategy(paymentStrategy);

               var report = reportBuilder.GetResult();

               order.ReportSnapshot = report;
               int dbId = await _orderRepo.SaveAsync(order);

               return (order, report, dbId);
          }

          private static OrderStatusChangedEvent BuildEvent(
               OrderDbTable order,
               string oldStatus,
               string newStatus,
               OrderStatusRequest req) => new()
          {
               OrderId             = order.Id,
               UserId              = order.UserId,
               OldStatus           = oldStatus,
               NewStatus           = newStatus,
               OrderTotal          = order.TotalAmount,
               Items               = order.Items.Select(i => new OrderEventItem(i.ProductId, i.ProductName, i.Quantity)).ToList(),
               RecipientContact    = req.RecipientContact,
               RecipientName       = req.RecipientName ?? "",
               NotificationChannel = req.NotificationChannel ?? "email"
          };

          private static OrderOperationResult Fail(int orderId, string message)
               => new() { Success = false, OrderId = orderId, Message = message };
     }
}
