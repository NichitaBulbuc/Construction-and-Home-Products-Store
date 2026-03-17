using CH_Store.Application.PaymentAdapter.API;
using CH_Store.Application.Payments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.PaymentAdapter.Processors
{
     public class StripePaymentAdapter : IPaymentProcessor
     {
          private readonly StripeExternalApi _stripeApi;

          public StripePaymentAdapter(StripeExternalApi stripeApi)
          {
               _stripeApi = stripeApi;
          }

          public void Process(double amount) // Metoda cerută de interfața ta
          {
               // Adaptăm datele: Stripe vrea long (cenți), noi avem double
               long amountInCents = (long)(amount * 100);

               // Apelăm metoda specifică a SDK-ului
               _stripeApi.CreatePaymentIntent(amountInCents, "MDL");
          }
     }
}
