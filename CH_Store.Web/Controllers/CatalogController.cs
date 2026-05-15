using CH_Store.Application.Product.Interfaces;
using CH_Store.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     /// <summary>
     /// Controller dedicat Composite Pattern — Catalog.
     ///
     /// Clientul (frontend / consumator API) vede catalogul ca o structura uniforma
     /// de noduri (produse + kituri) fara sa cunoasca implementarea interna.
     /// </summary>
     [ApiController]
     [Route("api/[controller]")]
     public class CatalogController : ControllerBase
     {
          private readonly ICatalogService _catalogService;

          public CatalogController(ICatalogService catalogService)
          {
               _catalogService = catalogService;
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/catalog
          // Catalogul complet: toate produsele (Leaf) + toate kiturile (Composite)
          // ──────────────────────────────────────────────────────────────
          [HttpGet]
          public async Task<IActionResult> GetFullCatalog()
          {
               var catalog = await _catalogService.GetFullCatalogAsync();
               return Ok(catalog);
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/catalog/kits
          // Lista tuturor kiturilor (fara arborele complet)
          // ──────────────────────────────────────────────────────────────
          [HttpGet("kits")]
          public async Task<IActionResult> GetAllKits()
          {
               var kits = await _catalogService.GetAllKitsAsync();
               return Ok(kits);
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/catalog/kit/{id}
          // Arborele complet al unui kit (recursiv, toate sub-kiturile incluse)
          // ──────────────────────────────────────────────────────────────
          [HttpGet("kit/{id:int}")]
          public async Task<IActionResult> GetKitTree(int id)
          {
               var tree = await _catalogService.GetKitTreeAsync(id);

               if (tree == null)
                    return NotFound(new { Error = $"Kitul cu ID {id} nu a fost gasit sau este inactiv." });

               return Ok(tree);
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/catalog/kit
          // Creeaza un kit nou gol
          // ──────────────────────────────────────────────────────────────
          [HttpPost("kit")]
          public async Task<IActionResult> CreateKit([FromBody] CreateKitRequest request)
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               int kitId = await _catalogService.CreateKitAsync(request);

               return CreatedAtAction(
                    nameof(GetKitTree),
                    new { id = kitId },
                    new { Message = $"Kitul '{request.Name}' a fost creat cu succes.", KitId = kitId });
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/catalog/kit/{id}/product
          // Adauga un produs (Leaf) la kit
          // ──────────────────────────────────────────────────────────────
          [HttpPost("kit/{id:int}/product")]
          public async Task<IActionResult> AddProductToKit(
               int id,
               [FromBody] AddProductToKitRequest request)
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               bool success = await _catalogService.AddProductToKitAsync(id, request);

               if (!success)
                    return NotFound(new
                    {
                         Error = $"Kitul ID={id} sau Produsul ID={request.ProductId} nu a fost gasit."
                    });

               return Ok(new
               {
                    Message = $"Produsul ID={request.ProductId} (x{request.Quantity}) a fost adaugat in kitul ID={id}."
               });
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/catalog/kit/{id}/subkit
          // Adauga un sub-kit (Composite copil) la kit
          // ──────────────────────────────────────────────────────────────
          [HttpPost("kit/{id:int}/subkit")]
          public async Task<IActionResult> AddSubKitToKit(
               int id,
               [FromBody] AddSubKitRequest request)
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               bool success = await _catalogService.AddSubKitToKitAsync(id, request);

               if (!success)
                    return BadRequest(new
                    {
                         Error = $"Operatia a esuat. Kitul ID={id} sau SubKitul ID={request.SubKitId} nu exista, " +
                                 $"sau adaugarea ar crea un ciclu in arborele Composite."
                    });

               return Ok(new
               {
                    Message = $"Sub-kitul ID={request.SubKitId} a fost adaugat in kitul ID={id}."
               });
          }

          // ──────────────────────────────────────────────────────────────
          // DELETE /api/catalog/kit/item/{itemId}
          // Sterge un element dintr-un kit (nu sterge produsul/sub-kitul)
          // ──────────────────────────────────────────────────────────────
          [HttpDelete("kit/item/{itemId:int}")]
          public async Task<IActionResult> RemoveKitItem(int itemId)
          {
               bool success = await _catalogService.RemoveKitItemAsync(itemId);

               if (!success)
                    return NotFound(new { Error = $"Elementul cu ID {itemId} nu a fost gasit." });

               return Ok(new { Message = $"Elementul ID={itemId} a fost eliminat din kit." });
          }

          // ──────────────────────────────────────────────────────────────
          // DELETE /api/catalog/kit/{id}
          // Dezactiveaza un kit (soft delete)
          // ──────────────────────────────────────────────────────────────
          [HttpDelete("kit/{id:int}")]
          public async Task<IActionResult> DeleteKit(int id)
          {
               bool success = await _catalogService.DeleteKitAsync(id);

               if (!success)
                    return NotFound(new { Error = $"Kitul cu ID {id} nu a fost gasit." });

               return Ok(new { Message = $"Kitul ID={id} a fost dezactivat din catalog." });
          }
     }
}
