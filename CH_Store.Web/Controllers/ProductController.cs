using CH_Store.Application.DBContext;
using CH_Store.Application.Product.Catalog;
using CH_Store.Application.Product.Services;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Models;
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
          [HttpGet("kit/{id}")]
          public IActionResult GetKitDetails(int id)
          {
               // 1. Simulăm extragerea din DB a produselor
               var ciocan = new IndividualProduct(new ProductPrototypeData
               { Id = 101, Name = "Ciocan Dulgher", Price = 85.5, Weight = 0.8 });

               var cuie = new IndividualProduct(new ProductPrototypeData
               { Id = 102, Name = "Set Cuie 100buc", Price = 20.0, Weight = 0.2 });

               // 2. Construim Kitul
               var kitReparatii = new ProductKit("Kit Reparații Acoperiș");
               kitReparatii.Add(ciocan);
               kitReparatii.Add(cuie);

               // 3. Putem adăuga chiar și un sub-kit
               var kitProtectie = new ProductKit("Set Protecție");
               kitProtectie.Add(new IndividualProduct(new ProductPrototypeData { Name = "Mănuși", Price = 15 }));

               kitReparatii.Add(kitProtectie);

               // 4. Returnăm un obiect care conține totul
               return Ok(new
               {
                    Id = id,
                    NumeKit = kitReparatii.Name,
                    PretTotal = kitReparatii.GetPrice(), // Calculat recursiv
                    Continut = kitReparatii.GetAllChildren() // Returnează lista de Componente
               });
          }

     }
}
