using CH_Store.Application.PaymentAdapter.API;
using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Models;

namespace CH_Store.Application.PaymentAdapter.Processors
{
     /// <summary>
     /// Adapter — adapteaza IStripeExternalApi (API extern incompatibil)
     /// la interfata IPaymentProcessor ceruta de Factory Method.
     ///
     /// Adapter Pattern:
     ///   Target    = IPaymentProcessor       (interfata asteptata de sistem)
     ///   Adaptee   = IStripeExternalApi       (interfata incompatibila a Stripe)
     ///   Adapter   = StripePaymentAdapter     (aceasta clasa — face bridging-ul)
     ///
     /// Diferentele adaptate:
     ///   • Suma:     double MDL  →  long cents  (Stripe lucreaza in unitati minime)
     ///   • Flux:     Process()   →  CreatePaymentIntent() + ConfirmPayment() + GetPaymentStatus()
     ///   • Rezultat: void/bool   →  PaymentResult (modelul intern al sistemului nostru)
     /// </summary>
     public class StripePaymentAdapter : IPaymentProcessor
     {
          private readonly IStripeExternalApi _stripeApi;

          public StripePaymentAdapter(IStripeExternalApi stripeApi)
          {
               _stripeApi = stripeApi;
          }

          /// <summary>
          /// Implementeaza IPaymentProcessor.Process() orchestrand fluxul Stripe:
          ///   1. Converteste suma MDL → cents (adaptare de tip)
          ///   2. Creeaza PaymentIntent la Stripe (obtine intent ID)
          ///   3. Confirma plata (autorizare la banca)
          ///   4. Verifica statusul final
          ///   5. Returneaza PaymentResult in formatul sistemului intern
          /// </summary>
          public PaymentResult Process(double amount)
          {
               // ── Pasul 1: Adaptare tip — MDL (double) → cents (long) ──────────
               long amountInCents = (long)Math.Round(amount * 100);
               string txId = $"STRIPE-PI-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

               Console.WriteLine($"[STRIPE ADAPTER] Initializare plata: {amount:F2} MDL ({amountInCents} cents)");
               Console.WriteLine($"[STRIPE ADAPTER] Referinta interna: {txId}");
               Console.WriteLine();

               // ── Pasul 2: Creare PaymentIntent ────────────────────────────────
               string intentId = _stripeApi.CreatePaymentIntent(amountInCents, "MDL");
               Console.WriteLine();

               // ── Pasul 3: Confirmare plata ────────────────────────────────────
               bool confirmed = _stripeApi.ConfirmPayment(intentId);
               Console.WriteLine();

               if (!confirmed)
               {
                    Console.WriteLine($"[STRIPE ADAPTER] Plata REFUZATA pentru {amount:F2} MDL. Intent: {intentId}");
                    return new PaymentResult
                    {
                         Success       = false,
                         Amount        = amount,
                         TransactionId = txId,
                         Message       = $"Plata de {amount:F2} MDL prin Stripe a fost REFUZATA. " +
                                        $"Motiv: card_declined (insufficient_funds). " +
                                        $"Stripe Intent ID: {intentId}",
                         ProcessedAt   = DateTime.UtcNow
                    };
               }

               // ── Pasul 4: Verificare status final ─────────────────────────────
               string status = _stripeApi.GetPaymentStatus(intentId);
               Console.WriteLine();

               // ── Pasul 5: Returnare rezultat in formatul sistemului intern ─────
               bool success = status == "succeeded";

               Console.WriteLine($"[STRIPE ADAPTER] Plata {(success ? "APROBATA" : "ESUATA")}. " +
                                 $"Suma: {amount:F2} MDL | Intent: {intentId} | Ref: {txId}");

               return new PaymentResult
               {
                    Success       = success,
                    Amount        = amount,
                    TransactionId = txId,
                    Message       = success
                         ? $"Plata de {amount:F2} MDL prin Stripe a fost procesata cu succes. " +
                           $"Fondurile au fost autorizate si transferate. " +
                           $"Stripe Intent ID: {intentId} | Referinta: {txId}"
                         : $"Plata de {amount:F2} MDL prin Stripe nu a putut fi finalizata. " +
                           $"Status Stripe: {status}. Stripe Intent ID: {intentId}",
                    ProcessedAt   = DateTime.UtcNow
               };
          }
     }
}
