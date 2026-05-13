using CH_Store.Application.Payments.Services;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Payment
{
     /// <summary>
     /// Concrete Strategy — plata cu cardul bancar (Visa / MasterCard).
     /// Delega executia catre CardPaymentService (Factory Method Pattern).
     /// </summary>
     public class CardPaymentStrategy : IPaymentStrategy
     {
          private readonly PaymentProvider _provider;

          public CardPaymentStrategy(PaymentProvider provider) => _provider = provider;

          public PaymentType PaymentType => PaymentType.Card;

          public string GetDescription() =>
               "Plata cu cardul bancar (Visa / MasterCard) — fonduri retinute instant";

          public PaymentResult Execute(decimal amount)
               => _provider.GetService(PaymentType.Card).Pay((double)amount, PaymentType.Card);
     }
}
