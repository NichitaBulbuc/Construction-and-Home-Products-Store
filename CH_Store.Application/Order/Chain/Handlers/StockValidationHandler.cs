using CH_Store.Application.DBContext;
using CH_Store.Application.Order.Chain;
using CH_Store.Domain.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Order.Chain.Handlers
{
     /// <summary>
     /// Handler 1/4 — Validare stoc.
     ///
     /// Verifica pentru fiecare produs din comanda daca
     /// StockQuantity din ProductDbTable >= cantitatea solicitata.
     ///
     /// Decizie:
     ///   • Orice produs fara stoc suficient  → Rejected  (opreste lantul)
     ///   • Produs inexistent in DB           → Rejected
     ///   • Toate produsele disponibile       → Approved  (continua lantul)
     ///
     /// Imbogatire context:
     ///   Populeaza context.ProductStocks (productId → StockQuantity)
     ///   pentru a fi reutilizat de handlere ulterioare fara query suplimentar.
     /// </summary>
     public class StockValidationHandler : OrderApprovalHandlerBase
     {
          private readonly AppDbContext _db;

          public StockValidationHandler(AppDbContext db) => _db = db;

          public override string HandlerName => "StockValidation";

          protected override async Task<ApprovalCheck> ValidateAsync(OrderApprovalContext context)
          {
               var productIds = context.Request.Items.Select(i => i.ProductId).Distinct().ToList();

               // Incarca toate produsele relevante intr-un singur query
               var products = await _db.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

               var errors = new List<string>();

               foreach (var item in context.Request.Items)
               {
                    if (!products.TryGetValue(item.ProductId, out var product))
                    {
                         errors.Add($"Produsul ID {item.ProductId} ('{item.ProductName}') nu exista in catalog.");
                         continue;
                    }

                    // Imbogatire context — reutilizat de FraudDetectionHandler
                    context.ProductStocks[item.ProductId] = product.StockQuantity;

                    if (product.StockQuantity < item.Quantity)
                    {
                         errors.Add(
                              $"'{product.Name}': solicitat {item.Quantity} buc, " +
                              $"disponibil {product.StockQuantity} buc.");
                    }
               }

               if (errors.Any())
                    return Reject(
                         $"Stoc insuficient pentru {errors.Count} produs(e): " +
                         string.Join(" | ", errors));

               int totalUnits = context.Request.Items.Sum(i => i.Quantity);
               return Approve(
                    $"Stoc verificat — toate cele {context.Request.Items.Count} produs(e) " +
                    $"({totalUnits} unit.) sunt disponibile.");
          }
     }
}
