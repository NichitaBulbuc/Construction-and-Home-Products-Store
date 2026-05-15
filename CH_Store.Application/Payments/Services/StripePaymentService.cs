using Application.DBContext;
using CH_Store.Application.PaymentAdapter.API;
using CH_Store.Application.PaymentAdapter.Processors;
using CH_Store.Application.Payments.Interfaces;

namespace CH_Store.Application.Payments.Services
{
     /// <summary>
     /// Concrete Creator (Factory Method) — creeaza un StripePaymentAdapter.
     ///
     /// Combina doua pattern-uri:
     ///   Factory Method : suprascrie Create() si returneaza IPaymentProcessor
     ///   Adapter        : procesorul returnat este StripePaymentAdapter,
     ///                    care adapteaza IStripeExternalApi la IPaymentProcessor
     ///
     /// IStripeExternalApi este injectat prin DI — nu mai este instantiat direct.
     /// Astfel StripePaymentService nu stie si nu depinde de implementarea concreta
     /// a API-ului Stripe (StripeExternalApi), respectand Dependency Inversion.
     /// </summary>
     public class StripePaymentService : PaymentService
     {
          private readonly IStripeExternalApi _stripeExternalApi;

          public StripePaymentService(PaymentContext context, IStripeExternalApi stripeExternalApi)
               : base(context)
          {
               _stripeExternalApi = stripeExternalApi;
          }

          public override IPaymentProcessor Create()
          {
               // Adapter Pattern: StripePaymentAdapter primeste IStripeExternalApi (nu concretul)
               return new StripePaymentAdapter(_stripeExternalApi);
          }
     }
}
