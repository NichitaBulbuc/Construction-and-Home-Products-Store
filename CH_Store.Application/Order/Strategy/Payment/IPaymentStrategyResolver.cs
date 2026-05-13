using CH_Store.Domain.Enums;

namespace CH_Store.Application.Order.Strategy.Payment
{
     /// <summary>
     /// Interfata resolver — abstractizeaza selectia strategiei de plata.
     /// Injectata in OrderFacade via DI; implementata de PaymentStrategyResolver.
     /// </summary>
     public interface IPaymentStrategyResolver
     {
          /// <summary>Returneaza strategia corespunzatoare tipului de plata.</summary>
          IPaymentStrategy Resolve(PaymentType type);

          /// <summary>Returneaza toate strategiile disponibile (pentru /strategies endpoint).</summary>
          IReadOnlyCollection<IPaymentStrategy> GetAll();
     }
}
