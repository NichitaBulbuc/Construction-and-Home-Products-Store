using CH_Store.Application.DBContext;
using CH_Store.Application.DbRepo;
using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Entities;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Orchestreaza clonarea unei comenzi folosind Prototype Pattern:
     ///
     ///  1. Preia template-ul din DB (OrderRepo)
     ///  2. Mapeaza OrderDbTable → OrderData
     ///  3. Creeaza OrderPrototype si cloneaza (deep copy + ajustari cantitati)
     ///  4. REFRESH PRICES — pretul fiecarui produs este citit din ProductDbTable (SQL Server)
     ///     → Bypaseaza cache-ul Proxy (ProductRemoteProxy) intentionat, preturile curente conteaza
     ///  5. Recalculeaza totalul cu noile preturi
     ///  6. Genereaza devizul comparativ (DevizGenerator)
     ///  7. Salveaza noua comanda via OrderRepo
     /// </summary>
     public class OrderTemplateService : IOrderTemplateService
     {
          private readonly IOrderRepo   _orderRepo;
          private readonly AppDbContext _db;

          private const decimal InstallationCost = 500m;
          private const decimal PriorityCost     = 200m;

          public OrderTemplateService(IOrderRepo orderRepo, AppDbContext db)
          {
               _orderRepo = orderRepo;
               _db        = db;
          }

          // ─── Clonare principala ───────────────────────────────────────────
          public async Task<CloneOrderResponse> CloneAsync(
               int templateOrderId,
               CloneOrderRequest request)
          {
               // Pasul 1 — Preia template-ul cu toate produsele incluse
               var templateEntity = await _orderRepo.GetByIdAsync(templateOrderId)
                    ?? throw new KeyNotFoundException(
                         $"Comanda-template cu ID {templateOrderId} nu exista in baza de date.");

               // Pasul 2 — Mapeaza entitatea DB la modelul de domeniu
               var templateData = MapToOrderData(templateEntity);

               // Pasul 3 — Prototype Pattern: deep copy + ajustari cantitati
               var prototype = new OrderPrototype(templateData);

               var clone = request.QuantityOverrides.Any()
                    ? prototype.CloneWithAdjustments(request.QuantityOverrides)
                    : prototype.Clone();

               // Suprascrieri optionale
               if (request.UserId > 0)
                    clone.UserId = request.UserId;

               if (!string.IsNullOrWhiteSpace(request.NewDeliveryAddress))
                    clone.DeliveryAddress = request.NewDeliveryAddress;

               if (!string.IsNullOrWhiteSpace(request.Notes))
                    clone.Notes = request.Notes;

               // Pasul 4 — REFRESH PRICES din ProductDbTable (SQL Server)
               // Intentionat bypass al ProductRemoteProxy — vrem preturile ACTUALE, nu cele din cache
               var priceUpdates = await RefreshPricesFromDbAsync(clone);

               // Pasul 5 — Recalculeaza totalul cu noile preturi
               RecalculateTotal(clone);

               // Pasul 6 — Genereaza devizul comparativ
               string deviz = DevizGenerator.Generate(templateEntity, clone, priceUpdates);
               clone.ReportSnapshot = deviz;

               // Pasul 7 — Salveaza noua comanda in DB
               int dbId = await _orderRepo.SaveAsync(clone);

               return new CloneOrderResponse
               {
                    DbId            = dbId,
                    TemplateOrderId = templateOrderId,
                    BuilderId       = clone.BuilderId,
                    OldTotal        = templateEntity.TotalAmount,
                    NewTotal        = clone.TotalPrice,
                    PriceUpdates    = priceUpdates,
                    Deviz           = deviz
               };
          }

          public Task<IEnumerable<OrderDbTable>> GetAllTemplatesAsync()
               => _orderRepo.GetAllAsync();

          // ─── Metode private ───────────────────────────────────────────────

          /// <summary>
          /// Citeste pretul curent din ProductDbTable pentru fiecare produs din clona.
          /// Daca produsul nu mai exista in DB, pretul vechi este pastrat si flagat.
          /// </summary>
          private async Task<List<ItemPriceUpdate>> RefreshPricesFromDbAsync(OrderData order)
          {
               var updates = new List<ItemPriceUpdate>();

               foreach (var item in order.Items)
               {
                    var dbProduct = await _db.Products.FindAsync(item.ProductId);

                    var update = new ItemPriceUpdate
                    {
                         ProductId    = item.ProductId,
                         ProductName  = item.ProductName,
                         Quantity     = item.Quantity,
                         OldPrice     = item.Price,
                         PriceNotFound = dbProduct == null
                    };

                    if (dbProduct != null)
                    {
                         update.NewPrice = dbProduct.Price; // pret curent din SQL Server
                         item.Price      = dbProduct.Price; // actualizam in clona
                    }
                    else
                    {
                         update.NewPrice = item.Price; // fallback — pret mentinut
                         Console.WriteLine(
                              $"[OrderPrototype] Produsul ID={item.ProductId} " +
                              $"({item.ProductName}) nu a fost gasit in ProductDbTable. " +
                              $"Se mentine pretul din template: {item.Price:F2} MDL.");
                    }

                    updates.Add(update);
               }

               return updates;
          }

          /// <summary>Recalculeaza totalul comenzii dupa actualizarea preturilor.</summary>
          private static void RecalculateTotal(OrderData order)
          {
               decimal total = (decimal)order.Items.Sum(i => i.Subtotal);

               total += order.DeliveryCost;
               if (order.HasInstallation) total += InstallationCost;
               if (order.IsPriority)      total += PriorityCost;
               total -= order.Discount;

               order.TotalPrice = total < 0 ? 0 : total;
          }

          /// <summary>Mapeaza OrderDbTable (entitate EF) → OrderData (model de domeniu).</summary>
          private static OrderData MapToOrderData(OrderDbTable entity)
          {
               // Parsam DeliveryType din string (salvat ca "Standard", "Express", etc.)
               var deliveryType = Enum.TryParse<DeliveryType>(entity.DeliveryType, ignoreCase: true, out var dt)
                    ? dt
                    : DeliveryType.Standard;

               return new OrderData
               {
                    UserId          = entity.UserId,
                    DeliveryAddress = entity.ShippingAddress,
                    DeliveryType    = deliveryType,
                    DeliveryCost    = entity.DeliveryCost,
                    HasInstallation = entity.HasInstallation,
                    IsPriority      = entity.IsPriority,
                    Discount        = entity.Discount,
                    Notes           = entity.Notes ?? "",
                    TotalPrice      = entity.TotalAmount,
                    Status          = entity.Status,
                    Items           = entity.Items.Select(i => new OrderItemData
                    {
                         ProductId   = i.ProductId,
                         ProductName = i.ProductName,
                         Price       = (double)i.UnitPrice,
                         Quantity    = i.Quantity
                    }).ToList()
               };
          }
     }
}
