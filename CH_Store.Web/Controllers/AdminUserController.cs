using CH_Store.Application.Admin.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     /// <summary>
     /// Admin — gestionarea utilizatorilor.
     ///
     /// GET /api/admin/users                  → lista paginata cu cautare
     /// GET /api/admin/users/{id}             → detalii user + statistici
     /// PUT /api/admin/users/{id}             → actualizare Email/Phone/Address/Level
     /// GET /api/admin/users/{id}/orders      → toate comenzile utilizatorului
     /// </summary>
     [ApiController]
     [Authorize(Roles = "Admin")]
     [Route("api/admin/users")]
     public class AdminUserController : ControllerBase
     {
          private readonly IAdminUserService _userService;

          public AdminUserController(IAdminUserService userService)
               => _userService = userService;

          // ── GET /api/admin/users ──────────────────────────────────────────────

          [HttpGet]
          public async Task<ActionResult<PagedResult<AdminUserSummary>>> GetAll(
               [FromQuery] int    page     = 1,
               [FromQuery] int    pageSize = 20,
               [FromQuery] string? search  = null)
          {
               var result = await _userService.GetAllAsync(page, pageSize, search);
               return Ok(result);
          }

          // ── GET /api/admin/users/{id} ─────────────────────────────────────────

          [HttpGet("{id:int}")]
          public async Task<IActionResult> GetById(int id)
          {
               var user = await _userService.GetByIdAsync(id);
               return user is null ? NotFound($"Utilizatorul #{id} nu a fost gasit.") : Ok(user);
          }

          // ── PUT /api/admin/users/{id} ─────────────────────────────────────────

          [HttpPut("{id:int}")]
          public async Task<IActionResult> Update(int id, [FromBody] AdminUserUpdateRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               var user = await _userService.UpdateAsync(id, request);
               return user is null ? NotFound($"Utilizatorul #{id} nu a fost gasit.") : Ok(user);
          }

          // ── GET /api/admin/users/{id}/orders ──────────────────────────────────

          [HttpGet("{id:int}/orders")]
          public async Task<IActionResult> GetUserOrders(int id)
          {
               // Verifica daca userul exista
               var user = await _userService.GetByIdAsync(id);
               if (user is null)
                    return NotFound($"Utilizatorul #{id} nu a fost gasit.");

               var orders = await _userService.GetUserOrdersAsync(id);
               return Ok(orders);
          }
     }
}
