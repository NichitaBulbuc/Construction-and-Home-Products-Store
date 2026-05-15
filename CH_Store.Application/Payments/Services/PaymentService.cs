using Application.DBContext;
using CH_Store.Application.Payments.Interfaces;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Payments.Services
{
     /// <summary>
     /// Abstract Creator — defineste Factory Method-ul si logica comuna de plata.
     ///
     /// Factory Method Pattern:
     ///   Create()  = Factory Method (abstract) — subclasele decid ce processor se creeaza
     ///   Pay()     = Template Method — apeleaza procesorul creat si salveaza tranzactia
     /// </summary>
     public abstract class PaymentService
     {
          private readonly PaymentContext _context;

          protected PaymentService(PaymentContext context)
          {
               _context = context;
          }

          /// <summary>
          /// Factory Method — fiecare Concrete Creator suprascrie aceasta metoda
          /// si returneaza procesorul specific metodei de plata.
          /// </summary>
          public abstract IPaymentProcessor Create();

          /// <summary>
          /// Template Method — fluxul de plata este intotdeauna acelasi:
          /// 1. Creeaza procesorul via Factory Method
          /// 2. Proceseaza plata (simulat)
          /// 3. Salveaza tranzactia in baza de date
          /// </summary>
          public PaymentResult Pay(double amount, PaymentType type)
          {
               // Pasul 1 — Factory Method decide ce processor se foloseste
               IPaymentProcessor processor = Create();

               // Pasul 2 — Simuleaza procesarea platii
               PaymentResult result = processor.Process(amount);

               // Pasul 3 — Persista tranzactia indiferent de rezultat
               _context.Transactions.Add(new PaymentData
               {
                    Amount        = amount,
                    Method        = type,
                    Status        = result.Success ? "Success" : "Failed",
                    TransactionId = result.TransactionId,
                    CreatedAt     = result.ProcessedAt
               });
               _context.SaveChanges();

               return result;
          }
     }
}
