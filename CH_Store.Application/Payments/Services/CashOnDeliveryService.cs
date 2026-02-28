using CH_Store.Application.Payments.Interfaces;
using CH_Store.Application.Payments.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Services
{
     public class CashOnDeliveryService : PaymentService
     {
          public override IPaymentProcessor Create()
          {
               return new CashOnDeliveryProcessor();
          }
     }
}
