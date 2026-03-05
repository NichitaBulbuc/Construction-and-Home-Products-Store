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
          [HttpPost("standard")]
          public IActionResult CreateStandardOrder([FromBody] OrderRequest dto)
          {
               if (dto == null) return BadRequest("Datele comenzii sunt invalide.");
               
               // Inițializăm Builder-ul și Directorul
               var orderBuilder = new OrderBuilder();
               var director = new OrderDirector(orderBuilder);

               // Directorul execută rețeta "Standard" folosind datele din DTO
               director.ConstructStandardOrder(dto);

               // Extragem produsul final
               OrderData finalOrder = orderBuilder.GetResult();

               var reportBuilder = new OrderReportBuilder();
               director = new OrderDirector(reportBuilder);

               director.ConstructStandardOrder(dto);
               string reportText = reportBuilder.GetResult();

               // În mod normal aici ai salva în DB: _repository.Save(order);
               return Ok(new { Message = "Comanda standard a fost creată", Order = finalOrder, Report = reportText });
          }

          [HttpPost("full")]
          public IActionResult CreateFullOrder([FromBody] OrderRequest dto)
          {
               if (dto == null) return BadRequest("Datele comenzii sunt invalide.");

               // Pasul 1: Construim obiectul Order real
               var orderBuilder = new OrderBuilder();
               var director = new OrderDirector(orderBuilder);

               director.ConstructFullOrder(dto);
               OrderData finalOrder = orderBuilder.GetResult();

               // Pasul 2: Generăm și un raport/sumar (folosind același Director dar alt Builder)
               var reportBuilder = new OrderReportBuilder();
               director = new OrderDirector(reportBuilder);

               director.ConstructFullOrder(dto);
               string reportText = reportBuilder.GetResult();

               return Ok(new
               {
                    Message = "Comanda completă a fost procesată",
                    Order = finalOrder,
                    Summary = reportText
               });
          }


     }
}
