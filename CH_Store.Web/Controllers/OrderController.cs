using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Strategy.Delivery;
using CH_Store.Application.Order.Strategy.Payment;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     [ApiController]
     [Authorize]
     [Route("api/[controller]")]
     public class OrderController : ControllerBase
     {
          private readonly IOrderFacade             _orderFacade;
          private readonly IOrderTemplateService    _templateService;
          private readonly IPaymentStrategyResolver _paymentStrategyResolver;

          public OrderController(
               IOrderFacade             orderFacade,
               IOrderTemplateService    templateService,
               IPaymentStrategyResolver paymentStrategyResolver)
          {
               _orderFacade             = orderFacade;
               _templateService         = templateService;
               _paymentStrategyResolver = paymentStrategyResolver;
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/order/standard
          // Comanda simpla: produse + livrare standard, fara extras.
          // ──────────────────────────────────────────────────────────────
          [HttpPost("standard")]
          public async Task<IActionResult> CreateStandardOrder([FromBody] OrderRequest dto)
          {
               if (dto == null || !dto.Items.Any())
                    return BadRequest("Comanda trebuie sa contina cel putin un produs.");

               var (order, report, dbId) = await _orderFacade.PlaceStandardOrderAsync(dto);

               return Ok(BuildResponse("Comanda standard salvata cu succes.", order, report, dbId));
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/order/full
          // Comanda completa: toate optiunile din request activate.
          // ──────────────────────────────────────────────────────────────
          [HttpPost("full")]
          public async Task<IActionResult> CreateFullOrder([FromBody] OrderRequest dto)
          {
               if (dto == null || !dto.Items.Any())
                    return BadRequest("Comanda trebuie sa contina cel putin un produs.");

               var (order, report, dbId) = await _orderFacade.PlaceFullOrderAsync(dto);

               return Ok(BuildResponse("Comanda completa salvata cu succes.", order, report, dbId));
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/order/express
          // Livrare Express + prioritate fortata.
          // ──────────────────────────────────────────────────────────────
          [HttpPost("express")]
          public async Task<IActionResult> CreateExpressOrder([FromBody] OrderRequest dto)
          {
               if (dto == null || !dto.Items.Any())
                    return BadRequest("Comanda trebuie sa contina cel putin un produs.");

               var (order, report, dbId) = await _orderFacade.PlaceExpressOrderAsync(dto);

               return Ok(BuildResponse("Comanda Express salvata cu succes.", order, report, dbId));
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/order/bulk
          // En-gros: reducere de 10% aplicata automat.
          // ──────────────────────────────────────────────────────────────
          [HttpPost("bulk")]
          public async Task<IActionResult> CreateBulkOrder([FromBody] OrderRequest dto)
          {
               if (dto == null || !dto.Items.Any())
                    return BadRequest("Comanda trebuie sa contina cel putin un produs.");

               var (order, report, dbId) = await _orderFacade.PlaceBulkOrderAsync(dto);

               return Ok(BuildResponse("Comanda en-gros salvata cu succes.", order, report, dbId));
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/order/pay
          // Facade complet: Builder + Payment (Factory Method + Adapter) +
          //                 Notification (Abstract Factory) + Persistenta.
          // ──────────────────────────────────────────────────────────────
          [HttpPost("pay")]
          public async Task<IActionResult> CreateOrderWithPayment([FromBody] OrderWithPaymentRequest dto)
          {
               if (dto == null || !dto.Items.Any())
                    return BadRequest("Comanda trebuie sa contina cel putin un produs.");

               var response = await _orderFacade.PlaceOrderWithPaymentAsync(dto);

               string statusMessage = response.PaymentSuccess
                    ? "Comanda a fost plasata si plata procesata cu succes."
                    : "Comanda a fost salvata, insa plata nu a putut fi procesata.";

               return Ok(new
               {
                    Message             = statusMessage,
                    Order               = response,
               });
          }

          // ──────────────────────────────────────────────────────────────
          // PATCH /api/order/{id}/status
          // Actualizeaza statusul comenzii + notificare automata.
          // ──────────────────────────────────────────────────────────────
          [HttpPatch("{id:int}/status")]
          public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusRequest request)
          {
               if (string.IsNullOrWhiteSpace(request?.NewStatus))
                    return BadRequest("Campul 'NewStatus' este obligatoriu.");

               var result = await _orderFacade.UpdateOrderStatusAsync(id, request);

               if (!result.Success)
                    return NotFound(new { Error = result.Message });

               return Ok(result);
          }

          // ──────────────────────────────────────────────────────────────
          // PATCH /api/order/{id}/cancel
          // Anuleaza comanda + notificare optionala.
          // ──────────────────────────────────────────────────────────────
          [HttpPatch("{id:int}/cancel")]
          public async Task<IActionResult> CancelOrder(int id, [FromBody] OrderStatusRequest? request = null)
          {
               var result = await _orderFacade.CancelOrderAsync(id, request);

               if (!result.Success)
                    return BadRequest(new { Error = result.Message });

               return Ok(result);
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/order/{id}
          // ──────────────────────────────────────────────────────────────
          [HttpGet("{id:int}")]
          public async Task<IActionResult> GetOrder(int id)
          {
               var order = await _orderFacade.GetOrderAsync(id);

               if (order == null)
                    return NotFound($"Comanda cu ID {id} nu a fost gasita.");

               return Ok(order);
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/order/user/{userId}
          // ──────────────────────────────────────────────────────────────
          [HttpGet("user/{userId:int}")]
          public async Task<IActionResult> GetOrdersByUser(int userId)
          {
               var orders = await _orderFacade.GetOrdersByUserAsync(userId);
               return Ok(orders);
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/order/templates
          // Lista tuturor comenzilor disponibile ca template (Prototype Pattern)
          // ──────────────────────────────────────────────────────────────
          [HttpGet("templates")]
          public async Task<IActionResult> GetTemplates()
          {
               var templates = await _templateService.GetAllTemplatesAsync();
               return Ok(templates);
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/order/clone/{templateId}
          // Prototype Pattern:
          //   Cloneaza comanda {templateId}, actualizeaza preturile din DB,
          //   ajusteaza cantitatile si genereaza devizul comparativ.
          // ──────────────────────────────────────────────────────────────
          [HttpPost("clone/{templateId:int}")]
          public async Task<IActionResult> CloneOrder(
               int templateId,
               [FromBody] CloneOrderRequest request)
          {
               try
               {
                    var response = await _templateService.CloneAsync(templateId, request);

                    return Ok(new
                    {
                         Message           = $"Comanda #{templateId} clonata cu succes. Noua comanda ID: {response.DbId}.",
                         NewOrderId        = response.DbId,
                         TemplateId        = response.TemplateOrderId,
                         OldTotal          = response.OldTotal,
                         NewTotal          = response.NewTotal,
                         Difference        = response.Difference,
                         DifferencePercent = $"{response.DifferencePercent:+0.00;-0.00}%",
                         ChangedPrices     = response.ChangedPricesCount,
                         PriceUpdates      = response.PriceUpdates,
                         Deviz             = response.Deviz
                    });
               }
               catch (KeyNotFoundException ex)
               {
                    return NotFound(new { Error = ex.Message });
               }
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/order/{id}/events
          // Observer audit log: istoricul evenimentelor unei comenzi
          // ──────────────────────────────────────────────────────────────
          [HttpGet("{id:int}/events")]
          public async Task<IActionResult> GetOrderEvents(int id)
          {
               var events = await _orderFacade.GetOrderEventsAsync(id);
               return Ok(events);
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/order/events
          // Observer audit log: toate evenimentele (admin dashboard)
          // ──────────────────────────────────────────────────────────────
          [HttpGet("events")]
          public async Task<IActionResult> GetAllEvents()
          {
               var events = await _orderFacade.GetAllEventsAsync();
               return Ok(events);
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/order/validate
          // Chain of Responsibility — validare comanda fara plasare
          // Ruleaza lantul complet: Stock → Credit → Discount → Fraud
          // ──────────────────────────────────────────────────────────────
          [HttpPost("validate")]
          public async Task<IActionResult> ValidateOrder([FromBody] OrderRequest dto)
          {
               if (dto == null || !dto.Items.Any())
                    return BadRequest("Comanda trebuie sa contina cel putin un produs.");

               var result = await _orderFacade.ValidateOrderAsync(dto);

               var statusCode = result.Status switch
               {
                    Domain.Enums.ApprovalStatus.Approved              => 200,
                    Domain.Enums.ApprovalStatus.PendingManagerApproval => 202,  // Accepted, but needs review
                    Domain.Enums.ApprovalStatus.Rejected              => 422,  // Unprocessable Entity
                    _                                                  => 200
               };

               return StatusCode(statusCode, new
               {
                    Status          = result.Status.ToString(),
                    IsApproved      = result.IsApproved,
                    NeedsReview     = result.NeedsReview,
                    IsRejected      = result.IsRejected,
                    RejectionReason = result.RejectionReason,
                    PendingReasons  = result.PendingReasons,
                    Checks          = result.Checks,
                    ProcessedAt     = result.ProcessedAt
               });
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/order/strategies
          // Strategy Pattern — lista tuturor strategiilor disponibile
          // ──────────────────────────────────────────────────────────────
          [HttpGet("strategies")]
          public IActionResult GetStrategies()
          {
               var delivery = DeliveryStrategyResolver.GetAll()
                    .Select(kvp => new
                    {
                         Type             = kvp.Key.ToString(),
                         Description      = kvp.Value.GetDescription(),
                         EstimatedDays    = kvp.Value.GetEstimatedDays(),
                         Cost             = kvp.Value.CalculateCost(new Domain.Models.OrderData()),
                         CostLabel        = kvp.Value.CalculateCost(new Domain.Models.OrderData()) == 0
                                            ? "gratuit"
                                            : $"+{kvp.Value.CalculateCost(new Domain.Models.OrderData())} MDL"
                    })
                    .OrderBy(s => s.Cost)
                    .ToList();

               var payment = _paymentStrategyResolver.GetAll()
                    .Select(s => new
                    {
                         Type        = s.PaymentType.ToString(),
                         Description = s.GetDescription()
                    })
                    .ToList();

               return Ok(new
               {
                    DeliveryStrategies = delivery,
                    PaymentStrategies  = payment
               });
          }

          // ─── Helper ─────────────────────────────────────────────────
          private static OrderResponse BuildResponse(
               string message,
               Domain.Models.OrderData order,
               string report,
               int dbId,
               Domain.DTOs.ApprovalResult? approval = null)
               => new()
               {
                    DbId                        = dbId,
                    BuilderId                   = order.BuilderId,
                    UserId                      = order.UserId,
                    Items                       = order.Items,
                    DeliveryAddress             = order.DeliveryAddress,
                    DeliveryType                = order.DeliveryType,
                    DeliveryCost                = order.DeliveryCost,
                    HasInstallation             = order.HasInstallation,
                    IsPriority                  = order.IsPriority,
                    Discount                    = order.Discount,
                    Notes                       = order.Notes,
                    TotalPrice                  = order.TotalPrice,
                    Status                      = order.Status,
                    CreatedAt                   = order.CreatedAt,
                    Report                      = report,
                    DeliveryStrategyDescription = order.DeliveryStrategyDescription,
                    EstimatedDeliveryDays       = order.EstimatedDeliveryDays,
                    ApprovalResult              = approval
               };
     }
}
