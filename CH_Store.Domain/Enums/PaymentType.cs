using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.Enums
{
     public enum PaymentType
     {
          Card,
          EWallet,
          BankTransfer,
          CashOnDelivery,
          Stripe
     }
}
