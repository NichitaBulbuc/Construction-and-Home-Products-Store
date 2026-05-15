using CH_Store.Application.Payments.Services;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Payment
{
     /// <summary>
     /// Concrete Strategy — procesare prin Stripe (Adapter Pattern intern).
     /// Delega executia catre StripePaymentService care foloseste StripePaymentAdapter.
     ///
     /// Colaborare patternuri:
     ///   Strategy  → decide ca se foloseste Stripe la runtime
     ///   Adapter   → StripePaymentAdapter traduce IPaymentProcessor → IStripeExternalApi
     ///   Factory Method → StripePaymentService.Create() instantiaza adaptorul
     /// </summary>
     public class StripePaymentStrategy : IPaymentStrategy
     {
          private readonly PaymentProvider _provider;

          public StripePaymentStrategy(PaymentProvider provider) => _provider = provider;

          public PaymentType PaymentType => PaymentType.Stripe;

          public string GetDescription() =>
               "Stripe — procesare internationala prin card, PaymentIntent + confirmare automata";

          public PaymentResult Execute(decimal amount)
               => _provider.GetService(PaymentType.Stripe).Pay((double)amount, PaymentType.Stripe);
     }
}
