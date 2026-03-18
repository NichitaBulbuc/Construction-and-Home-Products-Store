using CH_Store.Domain.DTOs;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Order.Interfaces
{
     public interface IOrderFacade
     {
          (OrderData Order, string Report) PlaceOrder(OrderRequest dto, bool isFullOrder);
     }
}
