using CH_Store.Domain.Models;

namespace CH_Store.Application.Payments.Interfaces
{
     /// <summary>
     /// Product — interfata comuna pentru toti procesatorii de plata.
     /// Fiecare implementare simuleaza metoda de plata si returneaza un rezultat cu mesaj.
     /// </summary>
     public interface IPaymentProcessor
     {
          PaymentResult Process(double amount);
     }
}
