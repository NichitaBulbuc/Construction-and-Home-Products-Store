using Application.DBContext;
using CH_Store.Application.PaymentAdapter.API;
using CH_Store.Application.PaymentAdapter.Processors;
using CH_Store.Application.Payments.Interfaces;

namespace CH_Store.Application.Payments.Services
{
     /// <summary>
     /// Concrete Creator — creeaza un StripePaymentAdapter.
     /// Foloseste pattern-ul Adapter pentru a adapta API-ul extern Stripe
     /// la interfata IPaymentProcessor ceruta de Factory Method.
     /// </summary>
     public class StripePaymentService : PaymentService
     {
          public StripePaymentService(PaymentContext context) : base(context) { }

          public override IPaymentProcessor Create()
          {
               var externalApi = new StripeExternalApi();
               return new StripePaymentAdapter(externalApi);
          }
     }
}
