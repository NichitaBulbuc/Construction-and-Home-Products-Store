using CH_Store.Application.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     /// <summary>
     /// Admin — statistici si monitoring in timp real.
     ///
     /// GET /api/admin/dashboard                      → snapshot complet (comenzi, venituri, produse, useri)
     /// GET /api/admin/dashboard/revenue?days=30      → venituri pe zile (ultimele N zile)
     /// GET /api/admin/dashboard/activity?count=50    → activitate recenta din OrderEventLogs
     /// GET /api/admin/dashboard/orders-by-status     → distributia comenzilor pe statusuri
     /// </summary>
     [ApiController]
     [Route("api/admin/dashboard")]
     public class AdminDashboardController : ControllerBase
     {
          private readonly IAdminDashboardService _dashboardService;

          public AdminDashboardController(IAdminDashboardService dashboardService)
               => _dashboardService = dashboardService;

          // ── GET /api/admin/dashboard ──────────────────────────────────────────

          [HttpGet]
          public async Task<IActionResult> GetStats()
          {
               var stats = await _dashboardService.GetStatsAsync();
               return Ok(stats);
          }

          // ── GET /api/admin/dashboard/revenue ─────────────────────────────────

          [HttpGet("revenue")]
          public async Task<IActionResult> GetRevenue([FromQuery] int days = 30)
          {
               if (days < 1 || days > 365)
                    return BadRequest("Parametrul 'days' trebuie sa fie intre 1 si 365.");

               var revenue = await _dashboardService.GetRevenueLastDaysAsync(days);
               return Ok(revenue);
          }

          // ── GET /api/admin/dashboard/activity ────────────────────────────────

          [HttpGet("activity")]
          public async Task<IActionResult> GetActivity([FromQuery] int count = 50)
          {
               if (count < 1 || count > 500)
                    return BadRequest("Parametrul 'count' trebuie sa fie intre 1 si 500.");

               var activity = await _dashboardService.GetRecentActivityAsync(count);
               return Ok(activity);
          }

          // ── GET /api/admin/dashboard/orders-by-status ────────────────────────

          [HttpGet("orders-by-status")]
          public async Task<IActionResult> GetOrdersByStatus()
          {
               var distribution = await _dashboardService.GetOrdersByStatusAsync();
               return Ok(distribution);
          }
     }
}
