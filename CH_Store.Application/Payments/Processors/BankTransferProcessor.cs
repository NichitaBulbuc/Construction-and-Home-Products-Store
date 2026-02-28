using CH_Store.Application.Payments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Processors
{
     public class BankTransferProcessor : IPaymentProcessor
     {
          public void Process(double amount)
          {
               Console.WriteLine($"[TRANSFER] Se proceseaza transfer in valoare de {amount} MDL.");
          }
     }
}
