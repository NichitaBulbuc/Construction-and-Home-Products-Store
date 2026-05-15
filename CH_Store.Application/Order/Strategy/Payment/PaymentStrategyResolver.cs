using CH_Store.Application.Payments.Services;
using CH_Store.Domain.Enums;

namespace CH_Store.Application.Order.Strategy.Payment
{
     /// <summary>
     /// Resolver (registry) pentru strategiile de plata.
     ///
     /// Colaboreaza cu Factory Method Pattern:
     ///   • PaymentProvider (Factory Method) creeaza procesoarele concrete
     ///   • Fiecare strategie primeste PaymentProvider si il apeleaza intern
     ///   • OrderFacade cunoaste doar IPaymentStrategyResolver — nu stie de procesoare
     ///
     /// Inregistrat ca Scoped in DI deoarece PaymentProvider este Scoped.
     ///
     /// Utilizare:
     ///   IPaymentStrategy strategy = _resolver.Resolve(PaymentType.Card);
     ///   PaymentResult result = strategy.Execute(totalAmount);
     /// </summary>
     public class PaymentStrategyResolver : IPaymentStrategyResolver
     {
          private readonly Dictionary<PaymentType, IPaymentStrategy> _strategies;

          public PaymentStrategyResolver(PaymentProvider paymentProvider)
          {
               _strategies = new Dictionary<PaymentType, IPaymentStrategy>
               {
                    [PaymentType.Card]           = new CardPaymentStrategy(paymentProvider),
                    [PaymentType.EWallet]        = new EWalletPaymentStrategy(paymentProvider),
                    [PaymentType.BankTransfer]   = new BankTransferPaymentStrategy(paymentProvider),
                    [PaymentType.CashOnDelivery] = new CashOnDeliveryPaymentStrategy(paymentProvider),
                    [PaymentType.Stripe]         = new StripePaymentStrategy(paymentProvider),
               };
          }

          /// <inheritdoc/>
          public IPaymentStrategy Resolve(PaymentType type)
          {
               return _strategies.TryGetValue(type, out var strategy)
                    ? strategy
                    : throw new ArgumentOutOfRangeException(
                         nameof(type),
                         $"Nicio strategie de plata inregistrata pentru tipul '{type}'.");
          }

          /// <inheritdoc/>
          public IReadOnlyCollection<IPaymentStrategy> GetAll()
               => _strategies.Values.ToList().AsReadOnly();
     }
}
