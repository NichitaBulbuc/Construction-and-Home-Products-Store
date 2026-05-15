using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Payments.Processors
{
     /// <summary>
     /// Concrete Product — simuleaza plata prin transfer bancar.
     /// </summary>
     public class BankTransferProcessor : IPaymentProcessor
     {
          private const string IbanDestination = "MD24AG000225100013104168";

          public PaymentResult Process(double amount)
          {
               string txId = $"BT-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";

               Console.WriteLine($"[TRANSFER] Initiere transfer bancar de {amount} MDL...");
               Console.WriteLine($"[TRANSFER] IBAN destinatar: {IbanDestination}");
               Console.WriteLine($"[TRANSFER] Ordin de plata inregistrat. Referinta: {txId}");

               return new PaymentResult
               {
                    Success       = true,
                    Amount        = amount,
                    TransactionId = txId,
                    Message       = $"Transfer bancar de {amount:F2} MDL initiat catre IBAN {IbanDestination}. " +
                                   $"Procesarea dureaza 1-3 zile lucratoare. Referinta: {txId}",
                    ProcessedAt   = DateTime.UtcNow
               };
          }
     }
}
