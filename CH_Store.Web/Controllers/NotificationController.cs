using CH_Store.Application.DBContext;
using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CH_Store.Web.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class NotificationController : ControllerBase
     {
          private readonly NotificationContext _context;
          private readonly Func<string, INotificationFactory> _factoryResolver;

          public NotificationController(NotificationContext context, Func<string, INotificationFactory> factoryResolver)
          {
               _context = context;
               _factoryResolver = factoryResolver;
          }

          [HttpPost("notify/{orderId}")]
          public async Task<IActionResult> NotifyOrder([FromBody] OrderNotificationRequest request)
          {
               // 1. Validare date primite
               if (request == null) return BadRequest();

               // 2. Căutăm comanda în baza de date In-Memory folosind ID-ul din request
               var order = await _context.Orders.FindAsync(request.OrderId);

               if (order == null)
                    return NotFound($"Comanda cu ID-ul {request.OrderId} nu există.");

               // 3. Abstract Factory: Alegem fabrica pe baza NotificationType din request ("sms" sau "email")
               INotificationFactory factory = _factoryResolver(request.NotificationType!);

               // Creăm familia de produse (Sender + Template)
               var sender = factory.CreateSender();
               var template = factory.CreateTemplate();

               // 4. Generăm conținutul
               // Putem folosi și datele din DB (order) și datele din Request (email/phone)
               string content = template.GetFormatedContent(order.ProductList);

               sender.Send(content);

               return Ok(new
               {
                    Success = true,
                    SentTo = request.NotificationType == "email" ? request.CustomerEmail : request.CustomerPhone
               });
          }

          // Endpoint auxiliar pentru a adăuga o comandă de test
          [HttpPost("create-test-order")]
          public async Task<IActionResult> CreateOrder([FromBody] OrderData order)
          {
               _context.Orders.Add(order);
               await _context.SaveChangesAsync();
               return Ok(order);
          }
     }
}
