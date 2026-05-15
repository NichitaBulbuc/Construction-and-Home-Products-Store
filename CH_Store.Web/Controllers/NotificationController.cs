using CH_Store.Application.Notifications.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class NotificationController : ControllerBase
     {
          private readonly INotificationService _notificationService;

          public NotificationController(INotificationService notificationService)
               => _notificationService = notificationService;

          // ──────────────────────────────────────────────────────────────────
          // POST /api/notification/send
          // Trimitere notificare generica (orice eveniment, orice canal).
          // Abstract Factory alege familia Sender + Template pe baza "channel".
          // ──────────────────────────────────────────────────────────────────
          [HttpPost("send")]
          public IActionResult Send([FromBody] OrderNotificationRequest request)
          {
               if (string.IsNullOrWhiteSpace(request.Recipient))
                    return BadRequest(new { Error = "Campul Recipient (email/telefon) este obligatoriu." });

               if (string.IsNullOrWhiteSpace(request.Channel))
                    return BadRequest(new { Error = "Canalul (email/sms) este obligatoriu." });

               var data = new NotificationData
               {
                    Event            = request.Event,
                    RecipientName    = request.RecipientName,
                    OrderId          = request.OrderId,
                    OrderTotal       = request.OrderTotal,
                    OrderStatus      = request.OrderStatus,
                    EstimatedDelivery = request.EstimatedDelivery,
                    PromoCode        = request.PromoCode,
                    PromoDescription = request.PromoDescription,
                    DiscountPercent  = request.DiscountPercent
               };

               _notificationService.Notify(data, request.Channel, request.Recipient);

               return Ok(new
               {
                    Success   = true,
                    Message   = $"Notificare '{request.Event}' trimisa prin {request.Channel.ToUpper()} catre {request.Recipient}."
               });
          }

          // ──────────────────────────────────────────────────────────────────
          // POST /api/notification/order-accepted
          // Shortcut: notificare la acceptarea comenzii
          // ──────────────────────────────────────────────────────────────────
          [HttpPost("order-accepted")]
          public IActionResult NotifyOrderAccepted([FromBody] OrderNotificationRequest request)
          {
               if (!request.OrderId.HasValue || !request.OrderTotal.HasValue)
                    return BadRequest(new { Error = "OrderId si OrderTotal sunt obligatorii pentru acest eveniment." });

               _notificationService.NotifyOrderAccepted(
                    request.OrderId.Value,
                    request.OrderTotal.Value,
                    request.RecipientName,
                    request.Channel,
                    request.Recipient);

               return Ok(new { Success = true, Message = $"Client notificat: comanda #{request.OrderId} acceptata." });
          }

          // ──────────────────────────────────────────────────────────────────
          // POST /api/notification/order-shipped
          // ──────────────────────────────────────────────────────────────────
          [HttpPost("order-shipped")]
          public IActionResult NotifyOrderShipped([FromBody] OrderNotificationRequest request)
          {
               if (!request.OrderId.HasValue)
                    return BadRequest(new { Error = "OrderId este obligatoriu." });

               _notificationService.NotifyOrderShipped(
                    request.OrderId.Value,
                    request.EstimatedDelivery ?? DateTime.UtcNow.AddDays(3),
                    request.RecipientName,
                    request.Channel,
                    request.Recipient);

               return Ok(new { Success = true, Message = $"Client notificat: comanda #{request.OrderId} expediata." });
          }

          // ──────────────────────────────────────────────────────────────────
          // POST /api/notification/promotion
          // ──────────────────────────────────────────────────────────────────
          [HttpPost("promotion")]
          public IActionResult NotifyPromotion([FromBody] OrderNotificationRequest request)
          {
               if (string.IsNullOrWhiteSpace(request.PromoCode))
                    return BadRequest(new { Error = "PromoCode este obligatoriu." });

               _notificationService.NotifyPromotion(
                    request.PromoCode!,
                    request.PromoDescription ?? "Oferta speciala CH Store!",
                    request.DiscountPercent ?? 10m,
                    request.Channel,
                    request.Recipient);

               return Ok(new { Success = true, Message = $"Notificare promotie '{request.PromoCode}' trimisa." });
          }

          // ──────────────────────────────────────────────────────────────────
          // GET /api/notification/events
          // Lista tuturor tipurilor de evenimente disponibile
          // ──────────────────────────────────────────────────────────────────
          [HttpGet("events")]
          public IActionResult GetEvents()
          {
               var events = Enum.GetValues<NotificationEvent>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() });

               return Ok(events);
          }

          // ──────────────────────────────────────────────────────────────────
          // GET /api/notification/log
          // Istoricul notificarilor trimise (InMemory)
          // ──────────────────────────────────────────────────────────────────
          [HttpGet("log")]
          public IActionResult GetLog()
               => Ok(_notificationService.GetLog());
     }
}
