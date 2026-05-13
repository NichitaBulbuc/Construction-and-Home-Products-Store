using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Models;
using CH_Store.Domain.Events;

namespace CH_Store.Application.Order.Interfaces
{
     /// <summary>
     /// Facade Pattern — interfata publica simpla care mascheaza complexitatea subsistemelor:
     ///
     ///   Builder subsystem   : OrderBuilder, OrderReportBuilder, OrderDirector
     ///   Payment subsystem   : PaymentProvider, PaymentService, StripePaymentAdapter (Adapter)
     ///   Notification subsystem : NotificationService, Abstract Factory (Email/SMS)
     ///   Persistence subsystem  : OrderRepo, AppDbContext
     ///
     /// Clientul (Controller) apeleaza o singura metoda si primeste rezultatul complet.
     /// Nu stie si nu trebuie sa stie cum sunt coordonate subsistemele intern.
     /// </summary>
     public interface IOrderFacade
     {
          // ─── Plasare comenzi (Builder + Persistenta) ─────────────────────────

          /// <summary>Construieste si salveaza o comanda standard (produse + livrare Standard).</summary>
          Task<(OrderData Order, string Report, int DbId)> PlaceStandardOrderAsync(OrderRequest dto);

          /// <summary>Construieste si salveaza o comanda completa cu toate optiunile din request.</summary>
          Task<(OrderData Order, string Report, int DbId)> PlaceFullOrderAsync(OrderRequest dto);

          /// <summary>Comanda cu livrare Express + prioritate fortata.</summary>
          Task<(OrderData Order, string Report, int DbId)> PlaceExpressOrderAsync(OrderRequest dto);

          /// <summary>Comanda en-gros cu reducere de 10% aplicata automat.</summary>
          Task<(OrderData Order, string Report, int DbId)> PlaceBulkOrderAsync(OrderRequest dto);

          // ─── Plasare comanda cu plata integrata (Builder + Payment + Notification + Persistenta) ──

          /// <summary>
          /// Fluxul complet al comenzii:
          ///   1. Builder construieste comanda (toate optiunile din request)
          ///   2. OrderRepo salveaza comanda in SQL Server
          ///   3. PaymentProvider proceseaza plata (Factory Method + Adapter pentru Stripe)
          ///   4. NotificationService trimite confirmare (Abstract Factory Email/SMS)
          ///
          /// Returneaza datele comenzii + rezultatul platii + statusul notificarii.
          /// </summary>
          Task<OrderWithPaymentResponse> PlaceOrderWithPaymentAsync(OrderWithPaymentRequest dto);

          // ─── Gestionare status ────────────────────────────────────────────────

          /// <summary>
          /// Actualizeaza statusul comenzii si trimite notificare automata daca
          /// datele de contact sunt prezente in request.
          ///
          /// Notificari automate per status:
          ///   "Processing" → NotifyOrderAccepted
          ///   "Shipped"    → NotifyOrderShipped (EstimatedDelivery = +3 zile)
          ///   "Delivered"  → NotifyOrderDelivered
          ///   "Cancelled"  → NotifyOrderCancelled
          /// </summary>
          Task<OrderOperationResult> UpdateOrderStatusAsync(int orderId, OrderStatusRequest request);

          /// <summary>
          /// Anuleaza comanda (seteaza status "Cancelled") si trimite notificare daca
          /// datele de contact sunt prezente.
          /// Returneaza false daca comanda nu exista sau este deja anulata.
          /// </summary>
          Task<OrderOperationResult> CancelOrderAsync(int orderId, OrderStatusRequest? notification = null);

          // ─── Interogari comenzi ──────────────────────────────────────────────

          /// <summary>Returneaza o comanda din DB dupa ID (cu toate produsele incluse).</summary>
          Task<OrderDbTable?> GetOrderAsync(int id);

          /// <summary>Returneaza toate comenzile unui user.</summary>
          Task<IEnumerable<OrderDbTable>> GetOrdersByUserAsync(int userId);

          // ─── Audit log Observer ───────────────────────────────────────────────

          /// <summary>Returneaza istoricul evenimentelor unei comenzi (Observer audit log).</summary>
          Task<IEnumerable<OrderEventLogDbTable>> GetOrderEventsAsync(int orderId);

          /// <summary>Returneaza toate evenimentele din audit log (admin dashboard).</summary>
          Task<IEnumerable<OrderEventLogDbTable>> GetAllEventsAsync();
     }
}
