using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Payments.Processors
{
     /// <summary>
     /// Concrete Product — simuleaza plata ramburs la livrare.
     /// </summary>
     public class CashOnDeliveryProcessor : IPaymentProcessor
     {
          public PaymentResult Process(double amount)
          {
               string txId = $"COD-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

               Console.WriteLine($"[RAMBURS] Comanda inregistrata. Suma de achitat la livrare: {amount} MDL.");
               Console.WriteLine($"[RAMBURS] Curierul va incasa suma la predarea coletului. Cod: {txId}");

               return new PaymentResult
               {
                    Success       = true,
                    Amount        = amount,
                    TransactionId = txId,
                    Message       = $"Comanda in valoare de {amount:F2} MDL a fost inregistrata cu plata ramburs. " +
                                   $"Suma va fi achitata curier la momentul livrarii. Cod confirmare: {txId}",
                    ProcessedAt   = DateTime.UtcNow
               };
          }
     }
}
