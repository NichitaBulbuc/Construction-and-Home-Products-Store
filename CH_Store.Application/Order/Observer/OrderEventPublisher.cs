using CH_Store.Domain.Events;

namespace CH_Store.Application.Order.Observer
{
     /// <summary>
     /// Observer Pattern — Concrete Subject.
     ///
     /// Mentine lista de observatori si ii notifica secvential la fiecare eveniment.
     ///
     /// Strategia de inregistrare — dubla:
     ///   1. DI auto-injection: toti IOrderObserver inregistrati in DI container sunt
     ///      injectati automat via IEnumerable&lt;IOrderObserver&gt; in constructor.
     ///      Aceasta este calea standard — observatorii sunt adaugati in Program.cs.
     ///
     ///   2. Subscribe/Unsubscribe: pentru observatori dinamici adaugati la runtime
     ///      (ex: un observer temporar pentru o sesiune specifica).
     ///
     /// Rezilienta: eroarea unui observator este prinsa si logata.
     /// Ceilalti observatori sunt notificati chiar daca unul esueaza.
     /// </summary>
     public class OrderEventPublisher : IOrderEventPublisher
     {
          private readonly List<IOrderObserver> _observers;

          /// <param name="observers">
          /// Injectati automat de DI — toti serviciile inregistrate ca IOrderObserver.
          /// </param>
          public OrderEventPublisher(IEnumerable<IOrderObserver> observers)
          {
               _observers = observers.ToList();
          }

          // ─── Gestionare observatori ───────────────────────────────────────────

          public void Subscribe(IOrderObserver observer)
          {
               if (!_observers.Contains(observer))
               {
                    _observers.Add(observer);
                    Console.WriteLine($"[Publisher] Observator inregistrat: {observer.ObserverName}");
               }
          }

          public void Unsubscribe(IOrderObserver observer)
          {
               if (_observers.Remove(observer))
                    Console.WriteLine($"[Publisher] Observator eliminat: {observer.ObserverName}");
          }

          // ─── Publicare eveniment ──────────────────────────────────────────────

          /// <summary>
          /// Notifica secvential toti observatorii activi.
          /// Fiecare eroare individuala este prinsa si logata — nu se propaga.
          /// </summary>
          public async Task PublishAsync(OrderStatusChangedEvent orderEvent)
          {
               Console.WriteLine();
               Console.WriteLine($"[Publisher] ══════════════════════════════════════════════");
               Console.WriteLine($"[Publisher] Eveniment: Comanda #{orderEvent.OrderId}");
               Console.WriteLine($"[Publisher] Status   : {orderEvent.OldStatus} → {orderEvent.NewStatus}");
               Console.WriteLine($"[Publisher] Total    : {orderEvent.OrderTotal:F2} MDL | {orderEvent.Items.Count} produs(e)");
               Console.WriteLine($"[Publisher] Observatori activi: {_observers.Count}");
               Console.WriteLine($"[Publisher] ──────────────────────────────────────────────");

               foreach (var observer in _observers)
               {
                    try
                    {
                         Console.WriteLine($"[Publisher] → {observer.ObserverName}...");
                         await observer.OnOrderStatusChangedAsync(orderEvent);
                    }
                    catch (Exception ex)
                    {
                         // Un observator care esueaza nu blocheaza ceilalti
                         Console.WriteLine($"[Publisher] ✗ {observer.ObserverName} a esuat: {ex.Message}");
                    }
               }

               Console.WriteLine($"[Publisher] ══════════════════════════════════════════════");
               Console.WriteLine();
          }
     }
}
