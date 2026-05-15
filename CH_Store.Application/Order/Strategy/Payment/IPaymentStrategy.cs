using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Strategy.Payment
{
     /// <summary>
     /// Strategy Pattern — interfata comuna pentru toate strategiile de plata.
     ///
     /// Fiecare strategie encapsuleaza un algoritm de plata distinct:
     ///   CardPaymentStrategy        → plata cu cardul bancar
     ///   EWalletPaymentStrategy     → portofel electronic
     ///   BankTransferPaymentStrategy → transfer bancar
     ///   CashOnDeliveryPaymentStrategy → plata la livrare
     ///   StripePaymentStrategy      → procesare prin Stripe (Adapter)
     ///
     /// Beneficii:
     ///   • OrderFacade nu mai stie de PaymentProvider — cunoaste doar IPaymentStrategy
     ///   • Adaugarea unui nou mod de plata = o noua clasa, fara if-uri in Facade
     ///   • Strategia se schimba la runtime dupa dto.PaymentMethod (rezolvata de PaymentStrategyResolver)
     ///   • Colaboreaza cu Factory Method (PaymentProvider) — Strategy il apeleaza intern
     /// </summary>
     public interface IPaymentStrategy
     {
          /// <summary>Tipul de plata reprezentat de aceasta strategie.</summary>
          PaymentType PaymentType { get; }

          /// <summary>Descriere lizibila afisata in raspuns API.</summary>
          string GetDescription();

          /// <summary>Executa plata si returneaza rezultatul.</summary>
          PaymentResult Execute(decimal amount);
     }
}
