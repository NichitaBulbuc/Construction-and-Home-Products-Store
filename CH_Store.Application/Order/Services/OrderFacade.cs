using CH_Store.Application.DbRepo;
using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Payments.Services;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Facade Pattern — punct unic de intrare care orchestreaza toate subsistemele:
     ///
     ///   ┌─────────────────────────────────────────────────────┐
     ///   │                   OrderFacade                        │
     ///   │                                                      │
     ///   │  ┌─────────────┐  ┌────────────┐  ┌─────────────┐  │
     ///   │  │   Builder   │  │  Payment   │  │Notification │  │
     ///   │  │  subsystem  │  │ subsystem  │  │  subsystem  │  │
     ///   │  │             │  │            │  │             │  │
     ///   │  │ OrderBuilder│  │PaymentProv.│  │Notification │  │
     ///   │  │ ReportBuild.│  │PaymentServ.│  │  Service    │  │
     ///   │  │ OrderDirector│ │StripeAdapt.│  │ AbstrFactory│  │
     ///   │  └─────────────┘  └────────────┘  └─────────────┘  │
     ///   │         │                │                │         │
     ///   │         └────────────────┴────────────────┘         │
     ///   │                          │                           │
     ///   │                   ┌─────────────┐                   │
     ///   │                   │  OrderRepo  │ (SQL Server)       │
     ///   │                   └─────────────┘                   │
     ///   └─────────────────────────────────────────────────────┘
     ///
     /// Controllerul apeleaza o singura metoda si primeste rezultatul complet.
     /// </summary>
     public class OrderFacade : IOrderFacade
     {
          private readonly IOrderRepo           _orderRepo;
          private readonly PaymentProvider      _paymentProvider;
          private readonly INotificationService _notificationService;

          public OrderFacade(
               IOrderRepo           orderRepo,
               PaymentProvider      paymentProvider,
               INotificationService notificationService)
          {
               _orderRepo           = orderRepo;
               _paymentProvider     = paymentProvider;
               _notificationService = notificationService;
          }

          // ════════════════════════════════════════════════════════════════════
          // PLASARE COMENZI — Builder + Persistenta
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
          // PLASARE COMANDA CU PLATA — Builder + Payment + Notification + Persistenta
          // ════════════════════════════════════════════════════════════════════

          /// <summary>
          /// Fluxul complet al comenzii cu plata integrata:
          ///
          ///   Pas 1 — Builder Pattern:
          ///     OrderBuilder + OrderReportBuilder construiesc datele comenzii
          ///     si raportul via OrderDirector (aceeasi reteta pentru ambii builderi).
          ///
          ///   Pas 2 — Persistenta:
          ///     OrderRepo salveaza comanda in SQL Server, genereaza ID-ul.
          ///
          ///   Pas 3 — Adapter + Factory Method:
          ///     PaymentProvider selecteaza Concrete Creator (ex: StripePaymentService)
          ///     care creeaza Adapter-ul (StripePaymentAdapter) si proceseaza plata.
          ///     Suma = TotalPrice al comenzii.
          ///
          ///   Pas 4 — Abstract Factory (Notification):
          ///     NotificationService trimite confirmare comenzii via canalul ales.
          ///     Daca plata a esuat, se trimite notificare de esec in loc de confirmare.
          /// </summary>
          public async Task<OrderWithPaymentResponse> PlaceOrderWithPaymentAsync(OrderWithPaymentRequest dto)
          {
               // ── Pas 1 + 2: Builder + Persistenta ──────────────────────────────
               var (order, report, dbId) = await BuildAndSaveAsync(dto, (d, r) => d.ConstructFullOrder(r));

               // ── Pas 3: Payment subsystem ───────────────────────────────────────
               bool   paymentSuccess = false;
               string paymentTxId    = "";
               string paymentMessage = "";

               try
               {
                    var paymentService = _paymentProvider.GetService(dto.PaymentMethod);
                    var paymentResult  = paymentService.Pay((double)order.TotalPrice, dto.PaymentMethod);

                    paymentSuccess = paymentResult.Success;
                    paymentTxId   = paymentResult.TransactionId;
                    paymentMessage = paymentResult.Message;
               }
               catch (Exception ex)
               {
                    paymentMessage = $"Eroare la procesarea platii: {ex.Message}";
               }

               // ── Pas 4: Notification subsystem ─────────────────────────────────
               bool notificationSent = false;

               if (!string.IsNullOrWhiteSpace(dto.RecipientContact))
               {
                    try
                    {
                         if (paymentSuccess)
                         {
                              _notificationService.NotifyOrderAccepted(
                                   orderId       : dbId,
                                   total         : order.TotalPrice,
                                   recipientName : dto.RecipientName,
                                   channel       : dto.NotificationChannel,
                                   recipient     : dto.RecipientContact);
                         }
                         else
                         {
                              // Plata esuata — notifica clientul
                              _notificationService.NotifyOrderCancelled(
                                   orderId       : dbId,
                                   recipientName : dto.RecipientName,
                                   channel       : dto.NotificationChannel,
                                   recipient     : dto.RecipientContact);

                              // Actualizeaza statusul comenzii la Cancelled daca plata a esuat
                              await _orderRepo.UpdateStatusAsync(dbId, "PaymentFailed");
                              order.Status = "PaymentFailed";
                         }

                         notificationSent = true;
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine($"[OrderFacade] Notificarea nu a putut fi trimisa: {ex.Message}");
                    }
               }

               // ── Asamblare raspuns complet ──────────────────────────────────────
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

                    PaymentSuccess       = paymentSuccess,
                    PaymentTransactionId = paymentTxId,
                    PaymentMessage       = paymentMessage,
                    PaymentMethod        = dto.PaymentMethod.ToString(),

                    NotificationSent    = notificationSent,
                    NotificationChannel = dto.NotificationChannel
               };
          }

          // ════════════════════════════════════════════════════════════════════
          // GESTIONARE STATUS — Persistenta + Notificare automata
          // ════════════════════════════════════════════════════════════════════

          /// <summary>
          /// Actualizeaza statusul comenzii si declanseaza notificarea corecta per status:
          ///   "Processing" → NotifyOrderAccepted  (confirmarea acceptarii comenzii)
          ///   "Shipped"    → NotifyOrderShipped   (cu estimare livrare +3 zile)
          ///   "Delivered"  → NotifyOrderDelivered
          ///   "Cancelled"  → NotifyOrderCancelled
          ///   Orice alt status → doar update in DB, fara notificare
          /// </summary>
          public async Task<OrderOperationResult> UpdateOrderStatusAsync(int orderId, OrderStatusRequest request)
          {
               if (string.IsNullOrWhiteSpace(request.NewStatus))
                    return Fail(orderId, "Statusul nou nu poate fi gol.");

               bool updated = await _orderRepo.UpdateStatusAsync(orderId, request.NewStatus);

               if (!updated)
                    return Fail(orderId, $"Comanda cu ID {orderId} nu a fost gasita.");

               bool notificationSent = false;

               if (request.HasNotificationData)
               {
                    try
                    {
                         string channel    = request.NotificationChannel!;
                         string recipient  = request.RecipientContact!;
                         string name       = request.RecipientName ?? "";

                         notificationSent = SendStatusNotification(
                              request.NewStatus, orderId, channel, recipient, name);
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine($"[OrderFacade] Notificarea status nu a putut fi trimisa: {ex.Message}");
                    }
               }

               return new OrderOperationResult
               {
                    Success          = true,
                    OrderId          = orderId,
                    NewStatus        = request.NewStatus,
                    NotificationSent = notificationSent,
                    Message          = $"Statusul comenzii #{orderId} a fost actualizat la '{request.NewStatus}'." +
                                       (notificationSent ? " Notificare trimisa." : "")
               };
          }

          /// <summary>
          /// Anuleaza comanda (status → "Cancelled") si trimite notificare daca
          /// datele de contact sunt disponibile in request.
          /// Nu poate anula o comanda deja anulata sau livrata.
          /// </summary>
          public async Task<OrderOperationResult> CancelOrderAsync(int orderId, OrderStatusRequest? notification = null)
          {
               // Verifica daca comanda exista si poate fi anulata
               var order = await _orderRepo.GetByIdAsync(orderId);

               if (order == null)
                    return Fail(orderId, $"Comanda cu ID {orderId} nu a fost gasita.");

               if (order.Status is "Cancelled" or "PaymentFailed")
                    return Fail(orderId, $"Comanda #{orderId} este deja anulata (status: {order.Status}).");

               if (order.Status == "Delivered")
                    return Fail(orderId, $"Comanda #{orderId} a fost deja livrata si nu poate fi anulata.");

               // Actualizeaza statusul
               await _orderRepo.UpdateStatusAsync(orderId, "Cancelled");

               bool notificationSent = false;

               // Trimite notificare daca sunt furnizate datele de contact
               var notifRequest = notification ?? new OrderStatusRequest();

               if (notifRequest.HasNotificationData)
               {
                    try
                    {
                         _notificationService.NotifyOrderCancelled(
                              orderId       : orderId,
                              recipientName : notifRequest.RecipientName ?? "",
                              channel       : notifRequest.NotificationChannel!,
                              recipient     : notifRequest.RecipientContact!);

                         notificationSent = true;
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine($"[OrderFacade] Notificarea de anulare nu a putut fi trimisa: {ex.Message}");
                    }
               }

               string reason = !string.IsNullOrWhiteSpace(notifRequest.Reason)
                    ? $" Motiv: {notifRequest.Reason}."
                    : "";

               return new OrderOperationResult
               {
                    Success          = true,
                    OrderId          = orderId,
                    NewStatus        = "Cancelled",
                    NotificationSent = notificationSent,
                    Message          = $"Comanda #{orderId} a fost anulata cu succes.{reason}" +
                                       (notificationSent ? " Notificare trimisa." : "")
               };
          }

          // ════════════════════════════════════════════════════════════════════
          // INTEROGARI
          // ════════════════════════════════════════════════════════════════════

          public Task<OrderDbTable?> GetOrderAsync(int id)
               => _orderRepo.GetByIdAsync(id);

          public Task<IEnumerable<OrderDbTable>> GetOrdersByUserAsync(int userId)
               => _orderRepo.GetByUserIdAsync(userId);

          // ════════════════════════════════════════════════════════════════════
          // METODE PRIVATE — logica interna
          // ════════════════════════════════════════════════════════════════════

          /// <summary>
          /// Nucleul Builder Pattern:
          /// Construieste OrderData si raportul text folosind aceeasi reteta (strategy)
          /// pe doi builderi diferiti, apoi salveaza rezultatul prin OrderRepo.
          /// </summary>
          private async Task<(OrderData Order, string Report, int DbId)> BuildAndSaveAsync(
               OrderRequest dto,
               Action<OrderDirector, OrderRequest> constructStrategy)
          {
               // ── Builder 1: OrderData ───────────────────────────────────────────
               var orderBuilder  = new OrderBuilder();
               var orderDirector = new OrderDirector(orderBuilder);
               constructStrategy(orderDirector, dto);
               var order = orderBuilder.GetResult();

               // ── Builder 2: Raport text ─────────────────────────────────────────
               var reportBuilder  = new OrderReportBuilder();
               var reportDirector = new OrderDirector(reportBuilder);
               constructStrategy(reportDirector, dto);
               var report = reportBuilder.GetResult();

               // ── Atasam raportul la comanda (persista ca snapshot in DB) ─────────
               order.ReportSnapshot = report;

               // ── Persistenta prin OrderRepo → AppDbContext → SQL Server ──────────
               int dbId = await _orderRepo.SaveAsync(order);

               return (order, report, dbId);
          }

          /// <summary>
          /// Alege si trimite notificarea corecta in functie de noul status al comenzii.
          /// Returneaza true daca notificarea a fost trimisa.
          /// </summary>
          private bool SendStatusNotification(
               string status, int orderId,
               string channel, string recipient, string recipientName)
          {
               switch (status.ToLower())
               {
                    case "processing":
                         // Comanda acceptata si in procesare — trimitem confirmare
                         _notificationService.NotifyOrderAccepted(
                              orderId       : orderId,
                              total         : 0m,      // total nu este disponibil la update status
                              recipientName : recipientName,
                              channel       : channel,
                              recipient     : recipient);
                         return true;

                    case "shipped":
                         // Comanda expediata — estimam livrarea la +3 zile lucratoare
                         _notificationService.NotifyOrderShipped(
                              orderId           : orderId,
                              estimatedDelivery : DateTime.UtcNow.AddDays(3),
                              recipientName     : recipientName,
                              channel           : channel,
                              recipient         : recipient);
                         return true;

                    case "delivered":
                         _notificationService.NotifyOrderDelivered(
                              orderId       : orderId,
                              recipientName : recipientName,
                              channel       : channel,
                              recipient     : recipient);
                         return true;

                    case "cancelled":
                         _notificationService.NotifyOrderCancelled(
                              orderId       : orderId,
                              recipientName : recipientName,
                              channel       : channel,
                              recipient     : recipient);
                         return true;

                    default:
                         // Statusuri custom (ex: "OnHold", "BackOrder") — fara notificare automata
                         return false;
               }
          }

          /// <summary>Helper pentru raspunsuri de eroare.</summary>
          private static OrderOperationResult Fail(int orderId, string message)
               => new() { Success = false, OrderId = orderId, Message = message };
     }
}
