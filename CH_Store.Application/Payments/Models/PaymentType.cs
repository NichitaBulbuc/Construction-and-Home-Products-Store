using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Payments.Models
{
     public enum PaymentType
     {
          Card,
          EWallet,
          BankTransfer,
          CashOnDelivery
     }
}
