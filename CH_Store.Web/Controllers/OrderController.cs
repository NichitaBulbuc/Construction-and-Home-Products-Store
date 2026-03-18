using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Services;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace CH_Store.Web.Controllers
{
     [ApiController]
     [Route("api/[controller]")]
     public class OrderController: ControllerBase
     {
          private readonly IOrderFacade _orderFacade;

          public OrderController(IOrderFacade orderFacade)
          {
               _orderFacade = orderFacade; 
          }

          [HttpPost("standard")]
          public IActionResult CreateStandardOrder([FromBody] OrderRequest dto)
          {
               if (dto == null) return BadRequest("Datele comenzii sunt invalide.");

               var result = _orderFacade.PlaceOrder(dto, isFullOrder: false);

               return Ok(new
               {
                    Message = "Comanda standard a fost creată",
                    Order = result.Order,
                    Report = result.Report
               });
          }

          [HttpPost("full")]
          public IActionResult CreateFullOrder([FromBody] OrderRequest dto)
          {
               if (dto == null) return BadRequest("Datele comenzii sunt invalide.");

               var result = _orderFacade.PlaceOrder(dto, isFullOrder: true);

               return Ok(new
               {
                    Message = "Comanda completă a fost procesată",
                    Order = result.Order,
                    Summary = result.Report
               });
          }


     }
}
