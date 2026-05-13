using CH_Store.Application.Admin.Interfaces;
using CH_Store.Domain.DTOs.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     /// <summary>
     /// Admin — CRUD produse + gestiunea stocului.
     ///
     /// GET    /api/admin/products                → lista (optional filtru category)
     /// POST   /api/admin/products                → adauga produs nou
     /// GET    /api/admin/products/{id}           → detalii produs
     /// PUT    /api/admin/products/{id}           → actualizare partiala produs
     /// DELETE /api/admin/products/{id}           → stergere (blocata daca exista comenzi active)
     /// PATCH  /api/admin/products/{id}/stock     → actualizeaza stocul
     /// GET    /api/admin/products/low-stock      → alerte stoc scazut (threshold=10)
     /// </summary>
     [ApiController]
     [Route("api/admin/products")]
     public class AdminProductController : ControllerBase
     {
          private readonly IAdminProductService _productService;

          public AdminProductController(IAdminProductService productService)
               => _productService = productService;

          // ── GET /api/admin/products ───────────────────────────────────────────

          [HttpGet]
          public async Task<IActionResult> GetAll([FromQuery] string? category = null)
          {
               var products = await _productService.GetAllAsync(category);
               return Ok(products);
          }

          // ── GET /api/admin/products/low-stock ────────────────────────────────
          // Definit INAINTE de {id} ca sa nu fie interceptat de ruta parametrizata

          [HttpGet("low-stock")]
          public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
          {
               var alerts = await _productService.GetLowStockAlertsAsync(threshold);
               return Ok(alerts);
          }

          // ── GET /api/admin/products/{id} ──────────────────────────────────────

          [HttpGet("{id:int}")]
          public async Task<IActionResult> GetById(int id)
          {
               var product = await _productService.GetByIdAsync(id);
               return product is null ? NotFound($"Produsul #{id} nu a fost gasit.") : Ok(product);
          }

          // ── POST /api/admin/products ──────────────────────────────────────────

          [HttpPost]
          public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               var product = await _productService.CreateAsync(request);
               return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
          }

          // ── PUT /api/admin/products/{id} ──────────────────────────────────────

          [HttpPut("{id:int}")]
          public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               var product = await _productService.UpdateAsync(id, request);
               return product is null ? NotFound($"Produsul #{id} nu a fost gasit.") : Ok(product);
          }

          // ── DELETE /api/admin/products/{id} ───────────────────────────────────

          [HttpDelete("{id:int}")]
          public async Task<IActionResult> Delete(int id)
          {
               try
               {
                    bool deleted = await _productService.DeleteAsync(id);
                    if (!deleted) return NotFound($"Produsul #{id} nu a fost gasit.");
                    return NoContent();
               }
               catch (InvalidOperationException ex)
               {
                    return Conflict(new { message = ex.Message });
               }
          }

          // ── PATCH /api/admin/products/{id}/stock ──────────────────────────────

          [HttpPatch("{id:int}/stock")]
          public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
          {
               if (!ModelState.IsValid) return BadRequest(ModelState);

               var product = await _productService.UpdateStockAsync(id, request);
               return product is null ? NotFound($"Produsul #{id} nu a fost gasit.") : Ok(product);
          }
     }
}
