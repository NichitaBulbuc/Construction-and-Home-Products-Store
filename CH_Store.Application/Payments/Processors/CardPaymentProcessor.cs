using CH_Store.Application.Payments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Processors
{
     public class CardPaymentProcessor : IPaymentProcessor
     {
          public void Process(double amount)
          {
               Console.WriteLine($"[CARD] Se proceseaza plata de {amount} MDL prin terminal bancar.");
          }
     }
}
