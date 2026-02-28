using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Interfaces
{
     public interface IPaymentProcessor
     {
          void Process(double amount);
     }
}
