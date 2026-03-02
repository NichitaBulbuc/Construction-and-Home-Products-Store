using Application.DBContext;
using CH_Store.Application.Payments.Services;
using CH_Store.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CH_Store.Web.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class PaymentsController : ControllerBase
     {
          private readonly PaymentProvider _paymentProvider;

          // Injectăm provider-ul prin constructor
          public PaymentsController(PaymentProvider paymentProvider)
          {
               _paymentProvider = paymentProvider;
          }

          [HttpPost("process")]
          public IActionResult ProcessPayment([FromBody] PaymentRequest request)
          {
               try
               {
                    // 1. Obținem serviciul corect din Factory
                    var service = _paymentProvider.GetService(request.Type);

                    // 2. Executăm plata
                    service.Pay(request.Amount, request.Type);

                    return Ok(new { Message = $"Plata de {request.Amount} prin {request.Type} a fost inițiată." });
               }
               catch (NotSupportedException ex)
               {
                    return BadRequest(new { Error = ex.Message });
               }
          }




     }

}
