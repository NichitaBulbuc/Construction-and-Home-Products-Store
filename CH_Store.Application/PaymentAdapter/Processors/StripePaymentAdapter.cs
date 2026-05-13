using CH_Store.Application.PaymentAdapter.API;
using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Models;

namespace CH_Store.Application.PaymentAdapter.Processors
{
     /// <summary>
     /// Adapter — adapteaza API-ul extern Stripe (CreatePaymentIntent cu long/cents)
     /// la interfata IPaymentProcessor ceruta de Factory Method (Process cu double/MDL).
     /// </summary>
     public class StripePaymentAdapter : IPaymentProcessor
     {
          private readonly StripeExternalApi _stripeApi;

          public StripePaymentAdapter(StripeExternalApi stripeApi)
          {
               _stripeApi = stripeApi;
          }

          public PaymentResult Process(double amount)
          {
               // Adaptare: double MDL -> long cents (Stripe lucreaza in unitati minime)
               long amountInCents = (long)(amount * 100);
               string txId = $"STRIPE-PI-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

               Console.WriteLine($"[STRIPE] Creare PaymentIntent pentru {amount} MDL ({amountInCents} cents)...");

               // Apel catre API-ul extern (simulat)
               _stripeApi.CreatePaymentIntent(amountInCents, "MDL");

               Console.WriteLine($"[STRIPE] PaymentIntent confirmat. ID: {txId}");

               return new PaymentResult
               {
                    Success       = true,
                    Amount        = amount,
                    TransactionId = txId,
                    Message       = $"Plata de {amount:F2} MDL prin Stripe a fost procesata. " +
                                   $"Fondurile au fost autorizate si transferate. Stripe Payment ID: {txId}",
                    ProcessedAt   = DateTime.UtcNow
               };
          }
     }
}
