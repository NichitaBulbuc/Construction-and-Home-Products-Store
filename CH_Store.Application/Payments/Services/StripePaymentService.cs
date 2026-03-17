using CH_Store.Application.PaymentAdapter.API;
using CH_Store.Application.PaymentAdapter.Processors;
using CH_Store.Application.Payments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Services
{
     public class StripePaymentService : PaymentService
     {
          public override IPaymentProcessor Create()
          {
               // În viața reală, StripeExternalApi ar veni probabil din DI
               var externalApi = new StripeExternalApi();

               return new StripePaymentAdapter(externalApi);
          }
     }
}
