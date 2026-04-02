using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.PaymentAdapter.API
{
     public class StripeExternalApi
     {
          public void CreatePaymentIntent(long amountInCents, string currency)
          {
               Console.WriteLine($"Plată Stripe procesată: {amountInCents} cenți.");
          }
     }
}
