using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Payments.Processors
{
     /// <summary>
     /// Concrete Product — simuleaza plata prin card bancar.
     /// </summary>
     public class CardPaymentProcessor : IPaymentProcessor
     {
          public PaymentResult Process(double amount)
          {
               string txId = $"CARD-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";

               Console.WriteLine($"[CARD] Initiere plata {amount} MDL...");
               Console.WriteLine($"[CARD] Se verifica fondurile disponibile...");
               Console.WriteLine($"[CARD] Tranzactie aprobata. ID: {txId}");

               return new PaymentResult
               {
                    Success       = true,
                    Amount        = amount,
                    TransactionId = txId,
                    Message       = $"Plata de {amount:F2} MDL prin card bancar a fost procesata cu succes. " +
                                   $"Suma a fost retinuta din cont. ID tranzactie: {txId}",
                    ProcessedAt   = DateTime.UtcNow
               };
          }
     }
}
