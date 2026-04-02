using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Payments.Interfaces;
using CH_Store.Application.Payments.Services;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Order.Services
{
     public class OrderFacade: IOrderFacade
     {

          public (OrderData Order, string Report) PlaceOrder(OrderRequest dto, bool isFullOrder)
          {
               Console.WriteLine("\n--- [FACADE] Începere procesare comandă ---");

               // --- PASUL 1: CONSTRUCȚIE ORDERDATA ---
               var orderBuilder = new OrderBuilder();
               var orderDirector = new OrderDirector(orderBuilder);

               if (isFullOrder)
               {
                    Console.WriteLine("[CHECKPOINT] Director: Execut rețeta 'Full Order' pentru date...");
                    orderDirector.ConstructFullOrder(dto);
               }
               else
               {
                    Console.WriteLine("[CHECKPOINT] Director: Execut rețeta 'Standard Order' pentru date...");
                    orderDirector.ConstructStandardOrder(dto);
               }

               var order = orderBuilder.GetResult();
               Console.WriteLine($"[CHECKPOINT] Builder: Obiect OrderData creat. Total de plată: {order.TotalPrice} MDL");

               // --- PASUL 2: GENERARE RAPORT ---
               var reportBuilder = new OrderReportBuilder();
               var reportDirector = new OrderDirector(reportBuilder);

               if (isFullOrder)
               {
                    Console.WriteLine("[CHECKPOINT] Director: Generare raport Full...");
                    reportDirector.ConstructFullOrder(dto);
               }
               else
               {
                    Console.WriteLine("[CHECKPOINT] Director: Generare raport Standard...");
                    reportDirector.ConstructStandardOrder(dto);
               }

               var report = reportBuilder.GetResult();
               Console.WriteLine("[CHECKPOINT] Builder: Raport text generat cu succes.");

               return (order, report);
          }
     }
}
