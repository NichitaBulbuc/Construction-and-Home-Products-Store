using CH_Store.Application.DbRepo;
using CH_Store.Application.Order.Chain;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Observer;
using CH_Store.Application.Order.Strategy.Delivery;
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
     /// Patternuri integrate (in ordinea executiei pentru PlaceOrderWithPaymentAsync):
     ///   Chain of Responsibility → IOrderApprovalChain  (validare/aprobare)
     ///   Builder                 → OrderBuilder / OrderDirector  (constructie)
     ///   Strategy (Delivery)     → DeliveryStrategyResolver  (cost livrare)
     ///   Strategy (Payment)      → IPaymentStrategyResolver  (executie plata)
     ///   Factory Method          → PaymentProvider (apelat intern de strategie)
     ///   Adapter                 → StripePaymentAdapter  (transparent)
     ///   Observer                → IOrderEventPublisher  (notificari, stoc, audit)
     ///   Persistence             → IOrderRepo  (SQL Server)
     /// </summary>
     public class OrderFacade : IOrderFacade
     {
          private readonly IOrderRepo              _orderRepo;
          private readonly IPaymentStrategyResolver _paymentStrategyResolver;
          private readonly IOrderEventPublisher    _eventPublisher;
          private readonly IOrderApprovalChain     _approvalChain;

          public OrderFacade(
               IOrderRepo               orderRepo,
               IPaymentStrategyResolver paymentStrategyResolver,
               IOrderEventPublisher     eventPublisher,
               IOrderApprovalChain      approvalChain)
          {
               _orderRepo               = orderRepo;
               _paymentStrategyResolver = paymentStrategyResolver;
               _eventPublisher          = eventPublisher;
               _approvalChain           = approvalChain;
          }

          // ════════════════════════════════════════════════════════════════════
          // VALIDARE STANDALONE — Chain of Responsibility
          // ════════════════════════════════════════════════════════════════════

          public async Task<ApprovalResult> ValidateOrderAsync(OrderRequest dto)
          {
               var context = BuildApprovalContext(dto);
               return await _approvalChain.ProcessAsync(context);
          }

          // ════════════════════════════════════════════════════════════════════
          // PLASARE COMENZI — Builder + Strategy (Delivery) + Chain + Persistenta
          // ════════════════════════════════════════════════════════════════════

          public async Task<(OrderData Order, string Report, int DbId)> PlaceStandardOrderAsync(OrderRequest dto)
               => await BuildValidateAndSaveAsync(dto, (d, r) => d.ConstructStandardOrder(r));

          public async Task<(OrderData Order, string Report, int DbId)> PlaceFullOrderAsync(OrderRequest dto)
               => await BuildValidateAndSaveAsync(dto, (d, r) => d.ConstructFullOrder(r));

          public async Task<(OrderData Order, string Report, int DbId)> PlaceExpressOrderAsync(OrderRequest dto)
               => await BuildValidateAndSaveAsync(dto, (d, r) => d.ConstructExpressOrder(r));

          public async Task<(OrderData Order, string Report, int DbId)> PlaceBulkOrderAsync(OrderRequest dto)
               => await BuildValidateAndSaveAsync(dto, (d, r) => d.ConstructBulkOrder(r));

          // ════════════════════════════════════════════════════════════════════
          // PLASARE COMANDA CU PLATA
          // Chain → Builder → Strategy(Payment) → Observer
          // ════════════════════════════════════════════════════════════════════

          public async Task<OrderWithPaymentResponse> PlaceOrderWithPaymentAsync(OrderWithPaymentRequest dto)
          {
               // ── Pas 1: Chain of Responsibility — validare/aprobare ─────────────
               var approvalContext = BuildApprovalContext(dto);
               var approval        = await _approvalChain.ProcessAsync(approvalContext);

               // Comanda respinsa → nu se salveaza, nu se proceseaza plata
               if (approval.IsRejected)
               {
                    return new OrderWithPaymentResponse
                    {
                         UserId          = dto.UserId,
                         Status          = "Rejected",
                         ApprovalResult  = approval,
                         PaymentSuccess  = false,
                         PaymentMessage  = "Comanda respinsa in etapa de validare — plata nu a fost initiata.",
                         CreatedAt       = DateTime.UtcNow
                    };
               }

               // ── Pas 2: Strategy Pattern — rezolva strategia de plata ───────────
               var paymentStrategy = _paymentStrategyResolver.Resolve(dto.PaymentMethod);

               // ── Pas 3: Builder + Persistenta ──────────────────────────────────
               // Daca PendingApproval → order salvat, plata AMANATA
               var (order, report, dbId) = await BuildAndSaveAsync(
                    dto,
                    (d, r) => d.ConstructFullOrder(r),
                    paymentStrategy,
                    approval.OrderStatusToSet);   // "New" sau "PendingApproval"

               // ── Pas 4: Plata — doar daca comanda este Approved ────────────────
               bool   paymentSuccess = false;
               string paymentTxId    = "";
               string paymentMessage = "";

               if (approval.IsApproved)
               {
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

                    // Actualizeaza status dupa plata
                    string newStatus = paymentSuccess ? "Processing" : "PaymentFailed";
                    await _orderRepo.UpdateStatusAsync(dbId, newStatus);
                    order.Status = newStatus;
               }
               else
               {
                    // PendingApproval — plata amanata pana la aprobare manager
                    paymentMessage =
                         "Plata amanata — comanda necesita aprobare manuala inainte de procesare.";
               }

               // ── Pas 5: Observer Pattern — publica eveniment ───────────────────
               await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
               {
                    OrderId             = dbId,
                    UserId              = order.UserId,
                    OldStatus           = "New",
                    NewStatus           = order.Status,
                    OrderTotal          = order.TotalPrice,
                    Items               = order.Items
                         .Select(i => new OrderEventItem(i.ProductId, i.ProductName, i.Quantity))
                         .ToList(),
                    RecipientContact    = dto.RecipientContact,
                    RecipientName       = dto.RecipientName,
                    NotificationChannel = dto.NotificationChannel
               });

               // ── Pas 6: Raspuns complet ────────────────────────────────────────
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

                    DeliveryStrategyDescription = order.DeliveryStrategyDescription,
                    EstimatedDeliveryDays       = order.EstimatedDeliveryDays,
                    PaymentStrategyDescription  = paymentStrategy.GetDescription(),

                    PaymentSuccess       = paymentSuccess,
                    PaymentTransactionId = paymentTxId,
                    PaymentMessage       = paymentMessage,
                    PaymentMethod        = dto.PaymentMethod.ToString(),

                    NotificationSent    = !string.IsNullOrWhiteSpace(dto.RecipientContact),
                    NotificationChannel = dto.NotificationChannel,

                    ApprovalResult = approval
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

               var req    = notification ?? new OrderStatusRequest();
               string reason = !string.IsNullOrWhiteSpace(req.Reason) ? $" Motiv: {req.Reason}." : "";
               await _eventPublisher.PublishAsync(BuildEvent(order, oldStatus, "Cancelled", req));

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
          /// Ruleaza lantul de aprobare, construieste si salveaza comanda.
          /// Folosit de metodele Place*OrderAsync (fara plata).
          /// </summary>
          private async Task<(OrderData Order, string Report, int DbId)> BuildValidateAndSaveAsync(
               OrderRequest dto,
               Action<OrderDirector, OrderRequest> recipe)
          {
               var approvalContext = BuildApprovalContext(dto);
               var approval        = await _approvalChain.ProcessAsync(approvalContext);

               if (approval.IsRejected)
                    throw new InvalidOperationException(
                         $"Comanda respinsa: {approval.RejectionReason}");

               return await BuildAndSaveAsync(dto, recipe, null, approval.OrderStatusToSet);
          }

          private async Task<(OrderData Order, string Report, int DbId)> BuildAndSaveAsync(
               OrderRequest dto,
               Action<OrderDirector, OrderRequest> recipe,
               IPaymentStrategy? paymentStrategy,
               string initialStatus = "New")
          {
               var orderBuilder  = new OrderBuilder();
               var orderDirector = new OrderDirector(orderBuilder);
               recipe(orderDirector, dto);

               if (paymentStrategy != null)
                    orderBuilder.SetPaymentStrategy(paymentStrategy);

               var order = orderBuilder.GetResult();
               order.Status = initialStatus;

               var reportBuilder  = new OrderReportBuilder();
               var reportDirector = new OrderDirector(reportBuilder);
               recipe(reportDirector, dto);

               if (paymentStrategy != null)
                    reportBuilder.SetPaymentStrategy(paymentStrategy);

               var report = reportBuilder.GetResult();
               order.ReportSnapshot = report;

               int dbId = await _orderRepo.SaveAsync(order);
               return (order, report, dbId);
          }

          /// <summary>
          /// Construieste OrderApprovalContext din cerere.
          /// Calculeaza totalurile folosind DeliveryStrategyResolver (Strategy Pattern)
          /// — aceeasi logica ca in Builder, fara a crea un OrderData complet.
          /// </summary>
          private static OrderApprovalContext BuildApprovalContext(OrderRequest dto)
          {
               decimal subtotal       = (decimal)dto.Items.Sum(i => i.Price * i.Quantity);
               var     deliveryStrat  = DeliveryStrategyResolver.Resolve(dto.DeliveryType);
               decimal deliveryCost   = deliveryStrat.CalculateCost(new OrderData());
               decimal installCost    = dto.WantsInstallation ? 500m : 0m;
               decimal priorityCost   = dto.IsPriority ? 200m : 0m;
               decimal total          = subtotal + deliveryCost + installCost + priorityCost - dto.Discount;

               return new OrderApprovalContext
               {
                    Request           = dto,
                    ComputedSubtotal  = subtotal,
                    ComputedTotal     = total < 0 ? 0 : total
               };
          }

          private static OrderStatusChangedEvent BuildEvent(
               OrderDbTable order, string oldStatus, string newStatus, OrderStatusRequest req) => new()
          {
               OrderId             = order.Id,
               UserId              = order.UserId,
               OldStatus           = oldStatus,
               NewStatus           = newStatus,
               OrderTotal          = order.TotalAmount,
               Items               = order.Items
                    .Select(i => new OrderEventItem(i.ProductId, i.ProductName, i.Quantity)).ToList(),
               RecipientContact    = req.RecipientContact,
               RecipientName       = req.RecipientName ?? "",
               NotificationChannel = req.NotificationChannel ?? "email"
          };

          private static OrderOperationResult Fail(int orderId, string message)
               => new() { Success = false, OrderId = orderId, Message = message };
     }
}
