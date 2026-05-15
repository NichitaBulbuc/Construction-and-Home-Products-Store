using CH_Store.Application.Payments.Services;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Payment
{
     /// <summary>
     /// Concrete Strategy — plata prin portofel electronic (MobilPay, PayNet etc.).
     /// Delega executia catre EWalletService (Factory Method Pattern).
     /// </summary>
     public class EWalletPaymentStrategy : IPaymentStrategy
     {
          private readonly PaymentProvider _provider;

          public EWalletPaymentStrategy(PaymentProvider provider) => _provider = provider;

          public PaymentType PaymentType => PaymentType.EWallet;

          public string GetDescription() =>
               "Portofel electronic (MobilPay / PayNet) — debitare instantanee din balanta";

          public PaymentResult Execute(decimal amount)
               => _provider.GetService(PaymentType.EWallet).Pay((double)amount, PaymentType.EWallet);
     }
}
