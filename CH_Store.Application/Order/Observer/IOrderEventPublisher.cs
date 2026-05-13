using CH_Store.Domain.Events;

namespace CH_Store.Application.Order.Observer
{
     /// <summary>
     /// Observer Pattern — interfata Subject (Observable).
     ///
     /// Permite inregistrarea/dezinregistrarea observatorilor si publicarea evenimentelor.
     ///
     /// In implementarea curenta, observatorii sunt injectati automat via DI
     /// (IEnumerable&lt;IOrderObserver&gt;), iar Subscribe/Unsubscribe sunt disponibili
     /// pentru observatori dinamici adaugati la runtime.
     /// </summary>
     public interface IOrderEventPublisher
     {
          /// <summary>Inregistreaza un observator dinamic (runtime).</summary>
          void Subscribe(IOrderObserver observer);

          /// <summary>Dezinregistreaza un observator.</summary>
          void Unsubscribe(IOrderObserver observer);

          /// <summary>
          /// Publica evenimentul catre toti observatorii inregistrati.
          /// Fiecare observator este notificat secvential.
          /// Eroarea unui observator nu opreste notificarea celorlalti.
          /// </summary>
          Task PublishAsync(OrderStatusChangedEvent orderEvent);
     }
}
