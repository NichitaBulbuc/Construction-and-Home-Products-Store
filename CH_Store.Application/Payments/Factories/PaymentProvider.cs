using CH_Store.Application.Payments.Models;
using CH_Store.Application.Payments.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Factories
{
     public class PaymentProvider
     {
          private readonly Dictionary<PaymentType, Func<PaymentService>> _creators = [];

          public PaymentProvider()
          {
               _creators[PaymentType.Card] = () => new CardPaymentService();
               _creators[PaymentType.EWallet] = () => new EWalletService();
               _creators[PaymentType.BankTransfer] = () => new BankTransferService();
               _creators[PaymentType.CashOnDelivery] = () => new CashOnDeliveryService();
          }

          public PaymentService GetService(PaymentType type)
          {
               if (!_creators.ContainsKey(type))
                    throw new NotSupportedException("Metoda de plată nu este disponibilă.");

               return _creators[type]();
          }


     }
}
