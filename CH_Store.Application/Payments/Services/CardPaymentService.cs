using CH_Store.Application.Payments.Interfaces;
using CH_Store.Application.Payments.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Services
{
     public class CardPaymentService : PaymentService
     {
          public override IPaymentProcessor Create()
          {
               return new CardPaymentProcessor();
          }
     }
}
