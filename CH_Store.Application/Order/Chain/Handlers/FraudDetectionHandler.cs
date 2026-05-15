using CH_Store.Application.DBContext;
using CH_Store.Application.Order.Chain;
using CH_Store.Domain.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.Order.Chain.Handlers
{
     /// <summary>
     /// Handler 4/4 — Detectare frauda / activitate suspecta.
     ///
     /// Verifica trei semnale de risc independent:
     ///   1. Valoare comanda > 100 000 MDL (tranzactie neobisnuit de mare)
     ///   2. Cantitate unitara > 100 buc per produs (posibil resel neautorizat)
     ///   3. Acelasi user a plasat > 5 comenzi in ultima ora (bot / card stolen)
     ///
     /// Decizie:
     ///   • Niciunul din semnale prezent → Approved
     ///   • Cel putin un semnal prezent  → PendingManagerApproval
     ///     (nu Rejected — poate fi legitim, dar necesita verificare umana)
     ///
     /// Nota: nu foloseste ProductStocks din context (deja populat de StockValidationHandler)
     /// deoarece cantitatea mare e semnalul independent de stoc.
     /// </summary>
     public class FraudDetectionHandler : OrderApprovalHandlerBase
     {
          private const decimal MaxSingleOrderAmount = 100_000m;
          private const int     MaxQuantityPerItem   = 100;
          private const int     MaxOrdersPerHour     = 5;

          private readonly AppDbContext _db;

          public FraudDetectionHandler(AppDbContext db) => _db = db;

          public override string HandlerName => "FraudDetection";

          protected override async Task<ApprovalCheck> ValidateAsync(OrderApprovalContext context)
          {
               var signals = new List<string>();

               // ── Semnal 1: Valoare exceptionala ───────────────────────────────
               if (context.ComputedTotal > MaxSingleOrderAmount)
                    signals.Add(
                         $"Valoare neobisnuita: {context.ComputedTotal:F2} MDL " +
                         $"(prag: {MaxSingleOrderAmount:N0} MDL)");

               // ── Semnal 2: Cantitati mari per produs ──────────────────────────
               var highQty = context.Request.Items
                    .Where(i => i.Quantity > MaxQuantityPerItem)
                    .ToList();

               if (highQty.Any())
               {
                    var details = string.Join(", ", highQty.Select(i => $"'{i.ProductName}' x{i.Quantity}"));
                    signals.Add($"Cantitati neobisnuite: {details} (prag/produs: {MaxQuantityPerItem})");
               }

               // ── Semnal 3: Frecventa comenzi pentru acelasi user ──────────────
               var windowStart = DateTime.UtcNow.AddHours(-1);
               int recentCount = await _db.Orders
                    .CountAsync(o =>
                         o.UserId    == context.Request.UserId &&
                         o.OrderDate >= windowStart);

               context.RecentOrdersCount = recentCount;

               if (recentCount >= MaxOrdersPerHour)
                    signals.Add(
                         $"Activitate neobisnuita: {recentCount} comenzi in ultima ora " +
                         $"pentru user {context.Request.UserId} (prag: {MaxOrdersPerHour})");

               // ── Decizie finala ────────────────────────────────────────────────
               if (signals.Any())
                    return Pending(
                         $"Semnale de risc detectate ({signals.Count}): " +
                         string.Join("; ", signals) +
                         ". Comanda marcata pentru verificare manuala.");

               return Approve(
                    $"Nicio activitate suspecta. " +
                    $"User {context.Request.UserId} are {recentCount} comenzi in ultima ora.");
          }
     }
}
