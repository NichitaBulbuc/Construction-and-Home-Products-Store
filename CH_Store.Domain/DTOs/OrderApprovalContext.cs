namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// Contextul passat de-a lungul intregului lant de aprobare.
     ///
     /// Contine:
     ///   • Date input (cererea clientului)
     ///   • Date calculate (totaluri, procent discount)
     ///   • Date imbogatite (stocuri incarcate de StockValidationHandler)
     ///   • Acumulatorul de verificari (adaugat de fiecare handler)
     ///
     /// Fiecare handler poate citi si scrie in context —
     /// handlerele ulterioare beneficiaza de datele incarcate de cele anterioare.
     /// Ex: StockValidationHandler incarca ProductStocks →
     ///     FraudDetectionHandler le poate refolosi fara query suplimentar.
     /// </summary>
     public class OrderApprovalContext
     {
          // ── Input (setat de Facade inainte de a porni lantul) ─────────────────
          public OrderRequest Request { get; set; } = null!;

          /// <summary>Suma produselor (fara discount, livrare, extras).</summary>
          public decimal ComputedSubtotal { get; set; }

          /// <summary>Total estimat (dupa discount + livrare + montaj + prioritate).</summary>
          public decimal ComputedTotal { get; set; }

          // ── Date imbogatite (populate de handlere pe parcurs) ─────────────────

          /// <summary>Stocuri reale din DB: productId → StockQuantity. Populat de StockValidationHandler.</summary>
          public Dictionary<int, int> ProductStocks { get; set; } = new();

          /// <summary>Procentul de discount calculat. Populat de DiscountApprovalHandler.</summary>
          public double DiscountPercent { get; set; }

          /// <summary>Numarul de comenzi recente ale userului (ultima ora). Populat de FraudDetectionHandler.</summary>
          public int RecentOrdersCount { get; set; }

          // ── Acumulator verificari (adaugat de fiecare handler) ─────────────────
          public List<ApprovalCheck> Checks { get; set; } = new();
     }
}
