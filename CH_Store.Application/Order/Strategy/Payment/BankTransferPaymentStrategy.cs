using CH_Store.Application.Payments.Services;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Payment
{
     /// <summary>
     /// Concrete Strategy — transfer bancar IBAN (procesare 1-3 zile lucratoare).
     /// Delega executia catre BankTransferService (Factory Method Pattern).
     /// </summary>
     public class BankTransferPaymentStrategy : IPaymentStrategy
     {
          private readonly PaymentProvider _provider;

          public BankTransferPaymentStrategy(PaymentProvider provider) => _provider = provider;

          public PaymentType PaymentType => PaymentType.BankTransfer;

          public string GetDescription() =>
               "Transfer bancar IBAN — procesare 1-3 zile lucratoare, fara comision";

          public PaymentResult Execute(decimal amount)
               => _provider.GetService(PaymentType.BankTransfer).Pay((double)amount, PaymentType.BankTransfer);
     }
}
