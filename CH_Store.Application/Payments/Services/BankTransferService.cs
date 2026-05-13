using Application.DBContext;
using CH_Store.Application.Payments.Interfaces;
using CH_Store.Application.Payments.Processors;

namespace CH_Store.Application.Payments.Services
{
     /// <summary>Concrete Creator — creeaza un BankTransferProcessor.</summary>
     public class BankTransferService : PaymentService
     {
          public BankTransferService(PaymentContext context) : base(context) { }

          public override IPaymentProcessor Create()
               => new BankTransferProcessor();
     }
}
