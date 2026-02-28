using CH_Store.Application.Payments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Processors
{
     public class CashOnDeliveryProcessor : IPaymentProcessor
     {
          public void Process(double amount)
          {
               Console.WriteLine($"[RAMBURS] Comanda ]n valoare de {amount} MDL va fi achitata la livrare.");
          }
     }
}
