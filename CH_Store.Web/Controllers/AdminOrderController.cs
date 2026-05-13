using CH_Store.Application.Admin.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.DTOs.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     /// <summary>
     /// Admin — gestionarea comenzilor.
     ///
     /// GET  /api/admin/orders                        → lista paginata + filtre
     /// GET  /api/admin/orders/pending-approval       → comenzile care asteapta aprobare
     /// GET  /api/admin/orders/{id}                   → detalii completa comanda
     /// POST /api/admin/orders/{id}/approve           → aprobare (+ plata optionala)
     /// POST /api/admin/orders/{id}/reject            → respingere cu motiv
     /// PATCH /api/admin/orders/{id}/status           → fortat orice status
     /// </summary>
     [ApiController]
     [Route("api/admin/orders")]
     public class AdminOrderController : ControllerBase
     {
          private readonly IAdminOrderService _orderService;

          public AdminOrderController(IAdminOrderService orderService)
               => _orderService = orderService;

          // ── GET /api/admin/orders ─────────────────────────────────────────────

          [HttpGet]
          public async Task<ActionResult<PagedResult<AdminOrderSummary>>> GetAll(
               [FromQuery] AdminOrderFilterRequest filter)
          {
               var result = await _orderService.GetAllAsync(filter);
               return Ok(result);
          }

          // ── GET /api/admin/orders/pending-approval ────────────────────────────
          // Definit INAINTE de {id} ca sa nu fie interceptat de ruta parametrizata

          [HttpGet("pending-approval")]
          public async Task<IActionResult> GetPendingApproval()
          {
               var orders = await _orderService.GetPendingApprovalAsync();
               return Ok(orders);
          }

          // ── GET /api/admin/orders/{id} ────────────────────────────────────────

          [HttpGet("{id:int}")]
          public async Task<IActionResult> GetDetail(int id)
          {
               var order = await _orderService.GetDetailAsync(id);
               return order is null ? NotFound($"Comanda #{id} nu a fost gasita.") : Ok(order);
          }

          // ── POST /api/admin/orders/{id}/approve ───────────────────────────────

          [HttpPost("{id:int}/approve")]
          public async Task<ActionResult<OrderOperationResult>> Approve(
               int id, [FromBody] AdminApproveOrderRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               var result = await _orderService.ApproveOrderAsync(id, request);

               if (!result.Success)
                    return BadRequest(result);

               return Ok(result);
          }

          // ── POST /api/admin/orders/{id}/reject ────────────────────────────────

          [HttpPost("{id:int}/reject")]
          public async Task<ActionResult<OrderOperationResult>> Reject(
               int id, [FromBody] AdminRejectOrderRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               var result = await _orderService.RejectOrderAsync(id, request);

               if (!result.Success)
                    return BadRequest(result);

               return Ok(result);
          }

          // ── PATCH /api/admin/orders/{id}/status ───────────────────────────────

          [HttpPatch("{id:int}/status")]
          public async Task<ActionResult<OrderOperationResult>> ForceStatus(
               int id, [FromBody] OrderStatusRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               var result = await _orderService.ForceUpdateStatusAsync(id, request);

               if (!result.Success)
                    return BadRequest(result);

               return Ok(result);
          }
     }
}
