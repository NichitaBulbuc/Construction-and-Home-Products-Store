using CH_Store.Application.Auth.Interfaces;
using CH_Store.Domain.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     /// <summary>
     /// Autentificare si inregistrare utilizatori.
     ///
     /// POST /api/auth/register  → cont nou (rol User implicit)
     /// POST /api/auth/login     → autentificare → JWT
     /// GET  /api/auth/me        → info utilizator curent (necesita JWT)
     /// </summary>
     [ApiController]
     [Route("api/auth")]
     public class AuthController : ControllerBase
     {
          private readonly IAuthService _authService;

          public AuthController(IAuthService authService)
               => _authService = authService;

          // ── POST /api/auth/register ───────────────────────────────────────────

          [HttpPost("register")]
          public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               try
               {
                    var response = await _authService.RegisterAsync(request);
                    return Ok(response);
               }
               catch (InvalidOperationException ex)
               {
                    return Conflict(new { message = ex.Message });
               }
          }

          // ── POST /api/auth/login ──────────────────────────────────────────────

          [HttpPost("login")]
          public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               var response = await _authService.LoginAsync(request);

               if (response == null)
                    return Unauthorized(new { message = "Username sau parolă incorectă." });

               return Ok(response);
          }

          // ── GET /api/auth/me ──────────────────────────────────────────────────

          [HttpGet("me")]
          [Authorize]
          public IActionResult Me()
          {
               return Ok(new
               {
                    userId   = User.FindFirst("userId")?.Value,
                    username = User.FindFirst("username")?.Value,
                    email    = User.FindFirst("email")?.Value,
                    role     = User.FindFirst("role")?.Value,
               });
          }
     }
}
