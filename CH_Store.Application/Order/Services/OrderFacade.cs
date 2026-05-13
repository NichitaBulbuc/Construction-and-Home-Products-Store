using CH_Store.Application.DbRepo;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Models;
using System.Collections.Generic;

namespace CH_Store.Application.Order.Services
{
     public class OrderFacade : IOrderFacade
     {
          private readonly IOrderRepo _orderRepo;

          public OrderFacade(IOrderRepo orderRepo)
               => _orderRepo = orderRepo;

          // ─── Metoda privata comuna — evita duplicarea ───────────────────
          private async Task<(OrderData Order, string Report, int DbId)> BuildAndSaveAsync(
               OrderRequest dto,
               Action<OrderDirector, OrderRequest> constructStrategy)
          {
               // Pasul 1: Construire date comanda
               var orderBuilder    = new OrderBuilder();
               var orderDirector   = new OrderDirector(orderBuilder);
               constructStrategy(orderDirector, dto);
               var order = orderBuilder.GetResult();

               // Pasul 2: Generare raport cu aceeasi reteta
               var reportBuilder   = new OrderReportBuilder();
               var reportDirector  = new OrderDirector(reportBuilder);
               constructStrategy(reportDirector, dto);
               var report = reportBuilder.GetResult();

               // Pasul 3: Atasam raportul la comanda (salvat in DB ca snapshot)
               order.ReportSnapshot = report;

               // Pasul 4: Persistenta prin OrderRepo -> AppDbContext -> SQL Server
               int dbId = await _orderRepo.SaveAsync(order);

               return (order, report, dbId);
          }

          // ─── Endpoint-uri publice ────────────────────────────────────────

          public Task<(OrderData Order, string Report, int DbId)> PlaceStandardOrderAsync(OrderRequest dto)
               => BuildAndSaveAsync(dto, (d, r) => d.ConstructStandardOrder(r));

          public Task<(OrderData Order, string Report, int DbId)> PlaceFullOrderAsync(OrderRequest dto)
               => BuildAndSaveAsync(dto, (d, r) => d.ConstructFullOrder(r));

          public Task<(OrderData Order, string Report, int DbId)> PlaceExpressOrderAsync(OrderRequest dto)
               => BuildAndSaveAsync(dto, (d, r) => d.ConstructExpressOrder(r));

          public Task<(OrderData Order, string Report, int DbId)> PlaceBulkOrderAsync(OrderRequest dto)
               => BuildAndSaveAsync(dto, (d, r) => d.ConstructBulkOrder(r));

          public Task<OrderDbTable?> GetOrderAsync(int id)
               => _orderRepo.GetByIdAsync(id);

          public Task<IEnumerable<OrderDbTable>> GetOrdersByUserAsync(int userId)
               => _orderRepo.GetByUserIdAsync(userId);
     }
}
