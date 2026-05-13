using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Payments.Processors
{
     /// <summary>
     /// Concrete Product — simuleaza plata prin portofel electronic (ex: MobilPay, PayNet).
     /// </summary>
     public class EWalletProcessor : IPaymentProcessor
     {
          public PaymentResult Process(double amount)
          {
               string txId = $"EW-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";

               Console.WriteLine($"[EWALLET] Conectare la portofel electronic...");
               Console.WriteLine($"[EWALLET] Verificare balanta: suficienta pentru {amount} MDL.");
               Console.WriteLine($"[EWALLET] Plata confirmata. ID: {txId}");

               return new PaymentResult
               {
                    Success       = true,
                    Amount        = amount,
                    TransactionId = txId,
                    Message       = $"Plata de {amount:F2} MDL prin portofel electronic a fost efectuata. " +
                                   $"Balanta portofelelului a fost actualizata. ID tranzactie: {txId}",
                    ProcessedAt   = DateTime.UtcNow
               };
          }
     }
}
