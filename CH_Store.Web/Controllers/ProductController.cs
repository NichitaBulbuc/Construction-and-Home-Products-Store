using CH_Store.Application.DBContext;
using CH_Store.Application.Product.Services;
using CH_Store.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace CH_Store.Web.Controllers
{
     [ApiController]
     [Route("api/[controller]")]
     public class ProductController: ControllerBase
     {
          private readonly ProductRegistry _registry;
          private readonly ProductContext _context;

          public ProductController(ProductRegistry registry, ProductContext context)
          {
               _registry = registry;
               _context = context;
          }

          [HttpPost("create-from-prototype")]
          public async Task<ActionResult<ProductResponse>> Create(ProductRequest request)
          {
               // 1. Regăsim prototipul din Registru
               var prototype = _registry.GetById(request.PrototypeType.ToLower());
               if (prototype == null) return NotFound("Prototipul nu există.");

               // 2. CLONĂM (Pattern Prototype în acțiune)
               var newProductClone = prototype.Clone();

               // 3. Aplicăm personalizările din DTO peste clonă
               var data = newProductClone.Data;
               if (!string.IsNullOrEmpty(request.CustomName)) data.Name = request.CustomName;
               if (request.OverriddenPrice.HasValue) data.Price = request.OverriddenPrice.Value;

               // 4. Salvare în baza de date
               _context.Products.Add(data);
               await _context.SaveChangesAsync();

               // 5. Mapare către Response DTO
               var response = new ProductResponse
               {
                    Id = data.Id,
                    Name = data.Name,
                    Price = data.Price,
                    Description = data.Description,
                    CategoryInfo = data.Weight > 0 ? $"{data.Weight} kg" : data.EnergyClass
               };

               return Ok(response);
          }

          [HttpGet("all-from-db")]
          public async Task<IActionResult> GetAll()
          {
               var products = await _context.Products.ToListAsync();
               return Ok(products);
          }

     }
}
