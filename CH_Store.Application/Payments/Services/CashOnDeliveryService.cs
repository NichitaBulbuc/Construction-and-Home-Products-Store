using Application.DBContext;
using CH_Store.Application.Payments.Interfaces;
using CH_Store.Application.Payments.Processors;

namespace CH_Store.Application.Payments.Services
{
     /// <summary>Concrete Creator — creeaza un CashOnDeliveryProcessor.</summary>
     public class CashOnDeliveryService : PaymentService
     {
          public CashOnDeliveryService(PaymentContext context) : base(context) { }

          public override IPaymentProcessor Create()
               => new CashOnDeliveryProcessor();
     }
}
