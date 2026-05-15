using Application.DBContext;
using CH_Store.Application.Payments.Interfaces;
using CH_Store.Application.Payments.Processors;

namespace CH_Store.Application.Payments.Services
{
     /// <summary>Concrete Creator — creeaza un CardPaymentProcessor.</summary>
     public class CardPaymentService : PaymentService
     {
          public CardPaymentService(PaymentContext context) : base(context) { }

          public override IPaymentProcessor Create()
               => new CardPaymentProcessor();
     }
}
