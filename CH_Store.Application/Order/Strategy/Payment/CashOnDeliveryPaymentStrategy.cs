using CH_Store.Application.Payments.Services;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Payment
{
     /// <summary>
     /// Concrete Strategy — plata ramburs la livrare (Cash on Delivery).
     /// Delega executia catre CashOnDeliveryService (Factory Method Pattern).
     /// </summary>
     public class CashOnDeliveryPaymentStrategy : IPaymentStrategy
     {
          private readonly PaymentProvider _provider;

          public CashOnDeliveryPaymentStrategy(PaymentProvider provider) => _provider = provider;

          public PaymentType PaymentType => PaymentType.CashOnDelivery;

          public string GetDescription() =>
               "Plata ramburs la livrare — suma achitata curier la predarea coletului";

          public PaymentResult Execute(decimal amount)
               => _provider.GetService(PaymentType.CashOnDelivery).Pay((double)amount, PaymentType.CashOnDelivery);
     }
}
