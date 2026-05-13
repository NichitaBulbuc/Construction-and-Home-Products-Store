using CH_Store.Application.Payments.Services;
using CH_Store.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class PaymentsController : ControllerBase
     {
          private readonly PaymentProvider _paymentProvider;

          public PaymentsController(PaymentProvider paymentProvider)
               => _paymentProvider = paymentProvider;

          /// <summary>
          /// Proceseaza o plata simulata.
          /// Factory Method Pattern:
          ///   PaymentProvider.GetService(type) -> returneaza Concrete Creator
          ///   service.Pay() -> apeleaza Create() (Factory Method) -> obtine Processor -> Process()
          /// </summary>
          [HttpPost("process")]
          public IActionResult ProcessPayment([FromBody] PaymentRequest request)
          {
               if (request.Amount <= 0)
                    return BadRequest(new { Error = "Suma trebuie sa fie mai mare decat 0." });

               try
               {
                    // Factory Method in actiune:
                    // GetService() returneaza Concrete Creator-ul potrivit tipului
                    var service = _paymentProvider.GetService(request.Type);

                    // Pay() apeleaza intern: Create() -> IPaymentProcessor -> Process()
                    var result = service.Pay(request.Amount, request.Type);

                    var response = new PaymentResponse
                    {
                         Success       = result.Success,
                         Message       = result.Message,
                         TransactionId = result.TransactionId,
                         Amount        = result.Amount,
                         PaymentMethod = request.Type.ToString(),
                         ProcessedAt   = result.ProcessedAt
                    };

                    return result.Success
                         ? Ok(response)
                         : UnprocessableEntity(response);
               }
               catch (ArgumentException ex)
               {
                    return BadRequest(new { Error = ex.Message });
               }
          }

          /// <summary>
          /// Returneaza toate metodele de plata disponibile.
          /// </summary>
          [HttpGet("methods")]
          public IActionResult GetPaymentMethods()
          {
               var methods = Enum.GetValues<CH_Store.Domain.Enums.PaymentType>()
                    .Select(t => new
                    {
                         Id          = (int)t,
                         Name        = t.ToString(),
                         Description = t switch
                         {
                              CH_Store.Domain.Enums.PaymentType.Card           => "Plata prin card Visa/Mastercard",
                              CH_Store.Domain.Enums.PaymentType.EWallet        => "Portofel electronic (MobilPay, PayNet)",
                              CH_Store.Domain.Enums.PaymentType.BankTransfer   => "Transfer bancar (1-3 zile lucratoare)",
                              CH_Store.Domain.Enums.PaymentType.CashOnDelivery => "Ramburs la livrare",
                              CH_Store.Domain.Enums.PaymentType.Stripe         => "Stripe — plata internationala",
                              _                                                 => ""
                         }
                    });

               return Ok(methods);
          }
     }
}
