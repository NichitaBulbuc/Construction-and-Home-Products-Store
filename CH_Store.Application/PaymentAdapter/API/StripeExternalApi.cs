namespace CH_Store.Application.PaymentAdapter.API
{
     /// <summary>
     /// Adaptee — simuleaza API-ul extern Stripe.
     ///
     /// Adapter Pattern:
     ///   Aceasta clasa reprezinta sistemul incompatibil (Stripe SDK) pe care
     ///   StripePaymentAdapter il adapteaza la interfata IPaymentProcessor.
     ///
     ///   API-ul Stripe real lucreaza cu:
     ///     - Sume in unitati minime (cents, bani) — nu in MDL cu zecimale
     ///     - Currency ca string ISO 4217 ("mdl", "usd", "eur")
     ///     - PaymentIntent ID-uri in format "pi_XXXXXXXX"
     ///     - Flux asincron in 2 pasi: CreatePaymentIntent → ConfirmPayment
     ///
     ///   Toate metodele sunt simulate (console output) — nu exista apel HTTP real.
     /// </summary>
     public class StripeExternalApi : IStripeExternalApi
     {
          // ─── CreatePaymentIntent ─────────────────────────────────────────────
          /// <summary>
          /// Simuleaza crearea unui PaymentIntent la Stripe.
          /// In productie: POST https://api.stripe.com/v1/payment_intents
          /// </summary>
          public string CreatePaymentIntent(long amountInCents, string currency)
          {
               string intentId = $"pi_{Guid.NewGuid().ToString("N")[..24].ToUpper()}";

               Console.WriteLine($"[STRIPE API] → POST /v1/payment_intents");
               Console.WriteLine($"[STRIPE API]   amount   : {amountInCents} {currency.ToUpper()} cents");
               Console.WriteLine($"[STRIPE API]   status   : requires_confirmation");
               Console.WriteLine($"[STRIPE API]   intent_id: {intentId}");

               return intentId;
          }

          // ─── ConfirmPayment ──────────────────────────────────────────────────
          /// <summary>
          /// Simuleaza confirmarea si autorizarea platii.
          /// In productie: POST https://api.stripe.com/v1/payment_intents/{id}/confirm
          /// </summary>
          public bool ConfirmPayment(string paymentIntentId)
          {
               // Simulare: 95% rata de succes (5% decline simulat)
               bool success = !paymentIntentId.EndsWith("0");

               if (success)
               {
                    Console.WriteLine($"[STRIPE API] → POST /v1/payment_intents/{paymentIntentId}/confirm");
                    Console.WriteLine($"[STRIPE API]   status   : succeeded");
                    Console.WriteLine($"[STRIPE API]   outcome  : authorized (network: visa)");
               }
               else
               {
                    Console.WriteLine($"[STRIPE API] → POST /v1/payment_intents/{paymentIntentId}/confirm");
                    Console.WriteLine($"[STRIPE API]   status   : canceled");
                    Console.WriteLine($"[STRIPE API]   error    : card_declined (insufficient_funds)");
               }

               return success;
          }

          // ─── GetPaymentStatus ────────────────────────────────────────────────
          /// <summary>
          /// Simuleaza interogarea statusului unui PaymentIntent.
          /// In productie: GET https://api.stripe.com/v1/payment_intents/{id}
          /// </summary>
          public string GetPaymentStatus(string paymentIntentId)
          {
               // Dupa confirmare intentul este mereu "succeeded" in simulare
               string status = "succeeded";

               Console.WriteLine($"[STRIPE API] → GET /v1/payment_intents/{paymentIntentId}");
               Console.WriteLine($"[STRIPE API]   status   : {status}");

               return status;
          }

          // ─── RefundPayment ───────────────────────────────────────────────────
          /// <summary>
          /// Simuleaza crearea unui Refund pentru o tranzactie procesata.
          /// In productie: POST https://api.stripe.com/v1/refunds
          /// </summary>
          public string RefundPayment(string paymentIntentId, long amountInCents)
          {
               string refundId = $"re_{Guid.NewGuid().ToString("N")[..24].ToUpper()}";

               Console.WriteLine($"[STRIPE API] → POST /v1/refunds");
               Console.WriteLine($"[STRIPE API]   payment_intent: {paymentIntentId}");
               Console.WriteLine($"[STRIPE API]   amount        : {amountInCents} cents");
               Console.WriteLine($"[STRIPE API]   refund_id     : {refundId}");
               Console.WriteLine($"[STRIPE API]   status        : succeeded");

               return refundId;
          }
     }
}
