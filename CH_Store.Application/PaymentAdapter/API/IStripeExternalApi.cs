namespace CH_Store.Application.PaymentAdapter.API
{
     /// <summary>
     /// Interfata Adaptee-ului Stripe.
     ///
     /// Adapter Pattern — aceasta este abstractizarea API-ului extern:
     ///   - In productie va fi implementata de un wrapper real peste Stripe SDK
     ///   - In simulare (acum) este implementata de StripeExternalApi (fictiv)
     ///   - StripePaymentAdapter depinde de aceasta interfata, nu de clasa concreta
     ///     → respecta Dependency Inversion Principle
     /// </summary>
     public interface IStripeExternalApi
     {
          /// <summary>
          /// Initializeaza o intentie de plata la Stripe.
          /// Returneaza un PaymentIntent ID generat de Stripe.
          /// </summary>
          string CreatePaymentIntent(long amountInCents, string currency);

          /// <summary>
          /// Confirma o intentie de plata initiata anterior.
          /// Returneaza true daca plata a fost autorizata cu succes.
          /// </summary>
          bool ConfirmPayment(string paymentIntentId);

          /// <summary>
          /// Returneaza statusul curent al unei tranzactii Stripe.
          /// Valori posibile: "requires_payment_method", "requires_confirmation",
          ///                  "processing", "succeeded", "canceled"
          /// </summary>
          string GetPaymentStatus(string paymentIntentId);

          /// <summary>
          /// Initiaza un refund pentru o tranzactie deja procesata.
          /// Returneaza un Refund ID generat de Stripe.
          /// </summary>
          string RefundPayment(string paymentIntentId, long amountInCents);
     }
}
