using CH_Store.Application.DBContext;
using CH_Store.Application.Product.Interfaces;
using CH_Store.Application.Product.Proxy;
using CH_Store.Application.Product.Services;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace CH_Store.Web.Controllers
{
     /// <summary>
     /// Controller pentru produse.
     ///
     /// Endpoint-urile de citire (GET) trec prin ProductRemoteProxy (Proxy Pattern):
     ///   Request → ProductRemoteProxy → [cache HIT] returneaza din IMemoryCache
     ///                                → [cache MISS] ProductDbService → SQL Server
     ///
     /// Endpoint-ul create-from-prototype foloseste Prototype Pattern (InMemory):
     ///   Request → ProductRegistry → Clone → ProductContext (InMemory)
     ///
     /// Endpoint-urile de catalog (Composite Pattern) sunt in CatalogController.
     /// </summary>
     [ApiController]
     [Route("api/[controller]")]
     public class ProductController : ControllerBase
     {
          private readonly ProductRegistry _registry;
          private readonly ProductContext  _prototypeContext;  // InMemory — Prototype Pattern
          private readonly IProductRepo    _repository;        // Proxy → SQL Server

          public ProductController(
               ProductRegistry registry,
               ProductContext  prototypeContext,
               IProductRepo    repository)
          {
               _registry         = registry;
               _prototypeContext  = prototypeContext;
               _repository       = repository;
          }

          // ──────────────────────────────────────────────────────────────
          // POST /api/product/create-from-prototype
          // Prototype Pattern: cloneaza un prototip si salveaza in InMemory DB
          // ──────────────────────────────────────────────────────────────
          [HttpPost("create-from-prototype")]
          public async Task<ActionResult<ProductResponse>> CreateFromPrototype(ProductRequest request)
          {
               var prototype = _registry.GetById(request.PrototypeType.ToLower());
               if (prototype == null)
                    return NotFound("Prototipul nu exista.");

               var clone = prototype.Clone();
               var data  = clone.Data;

               if (!string.IsNullOrEmpty(request.CustomName))   data.Name  = request.CustomName;
               if (request.OverriddenPrice.HasValue)             data.Price = request.OverriddenPrice.Value;

               _prototypeContext.Products.Add(data);
               await _prototypeContext.SaveChangesAsync();

               return Ok(new ProductResponse
               {
                    Id           = data.Id,
                    Name         = data.Name,
                    Price        = data.Price,
                    Description  = data.Description,
                    CategoryInfo = data.Weight > 0 ? $"{data.Weight} kg" : data.EnergyClass
               });
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/product
          // Proxy Pattern: toate produsele din SQL Server (cache → DB)
          // ──────────────────────────────────────────────────────────────
          [HttpGet]
          public async Task<ActionResult<IEnumerable<ProductDbTable>>> GetAll()
          {
               var products = await _repository.GetAllAsync();
               return Ok(products);
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/product/{id}
          // Proxy Pattern: produs dupa ID (cache → DB)
          // ──────────────────────────────────────────────────────────────
          [HttpGet("{id:int}")]
          public async Task<ActionResult<ProductDbTable>> GetById(int id)
          {
               var product = await _repository.GetByIdAsync(id);

               if (product == null)
                    return NotFound(new { Message = $"Produsul cu ID {id} nu a fost gasit." });

               return Ok(product);
          }

          // ──────────────────────────────────────────────────────────────
          // GET /api/product/category/{category}
          // Proxy Pattern: produse filtrate dupa categorie (cache → DB)
          // ──────────────────────────────────────────────────────────────
          [HttpGet("category/{category}")]
          public async Task<ActionResult<IEnumerable<ProductDbTable>>> GetByCategory(string category)
          {
               var products = await _repository.GetByCategoryAsync(category);
               return Ok(products);
          }

          // ──────────────────────────────────────────────────────────────
          // DELETE /api/product/cache
          // Invalideaza tot cache-ul de produse (toate request-urile afectate)
          // ──────────────────────────────────────────────────────────────
          [HttpDelete("cache")]
          public IActionResult InvalidateCache()
          {
               ProductRemoteProxy.InvalidateAll();
               return Ok(new { Message = "Cache-ul de produse a fost invalidat. Urmatorul request va incarca din SQL Server." });
          }

          // ──────────────────────────────────────────────────────────────
          // DELETE /api/product/{id}/cache
          // Invalideaza cache-ul pentru un produs specific
          // ──────────────────────────────────────────────────────────────
          [HttpDelete("{id:int}/cache")]
          public IActionResult InvalidateProductCache(int id)
          {
               if (_repository is ProductRemoteProxy proxy)
               {
                    proxy.InvalidateProduct(id);
                    return Ok(new { Message = $"Cache-ul pentru produsul ID={id} a fost invalidat." });
               }

               return Ok(new { Message = "Proxy cache nu este activ." });
          }
     }
}
