using CH_Store.Domain.Events;

namespace CH_Store.Application.Order.Observer
{
     /// <summary>
     /// Observer Pattern — interfata Observer.
     ///
     /// Fiecare observator concret implementeaza aceasta interfata.
     /// Subject (IOrderEventPublisher) apeleaza OnOrderStatusChangedAsync
     /// pe toti observatorii inregistrati cand statusul unei comenzi se schimba.
     ///
     /// Observatori implementati:
     ///   • CustomerNotificationObserver — email/SMS via Abstract Factory
     ///   • StockObserver               — actualizare stocuri in SQL Server
     ///   • AdminDashboardObserver      — audit log in OrderEventLogDbTable
     /// </summary>
     public interface IOrderObserver
     {
          /// <summary>Numele observatorului — folosit in logging si audit trail.</summary>
          string ObserverName { get; }

          /// <summary>
          /// Apelat de Subject cand statusul unei comenzi se schimba.
          /// Fiecare observator reactioneaza conform responsabilitatii sale.
          /// </summary>
          Task OnOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent);
     }
}
