using CH_Store.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Domain.DTOs
{
     public record PaymentRequest(PaymentType Type, double Amount);
     
}
