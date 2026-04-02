using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Services
{
     public class PaymentProvider
     {
          private readonly Dictionary<PaymentType, Func<PaymentService>> _creators = new();
          private readonly IServiceProvider _serviceProvider;

          public PaymentProvider(IServiceProvider serviceProvider)
          {
               _serviceProvider = serviceProvider;

               // Mapăm fiecare tip de plată către o funcție care creează serviciul
               // ActivatorUtilities injectează automat DbContext-ul în constructorul serviciului
               _creators[PaymentType.Card] = () =>
                   ActivatorUtilities.CreateInstance<CardPaymentService>(_serviceProvider);

               _creators[PaymentType.EWallet] = () =>
                   ActivatorUtilities.CreateInstance<EWalletService>(_serviceProvider);

               _creators[PaymentType.BankTransfer] = () =>
                   ActivatorUtilities.CreateInstance<BankTransferService>(_serviceProvider);

               _creators[PaymentType.CashOnDelivery] = () =>
                   ActivatorUtilities.CreateInstance<CashOnDeliveryService>(_serviceProvider);
               _creators[PaymentType.Stripe] = () =>
                   ActivatorUtilities.CreateInstance<StripePaymentService>(_serviceProvider);

          }

          public PaymentService GetService(PaymentType type)
          {
               if (!_creators.ContainsKey(type))
                    throw new ArgumentException("Tip de plată nesuportat!");

               return _creators[type]();
          }
     }
}
