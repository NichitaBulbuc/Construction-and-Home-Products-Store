using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Order.Services
{
     public class OrderDirector
     {
          private IOrderBuilder _builder;

          public OrderDirector(IOrderBuilder builder)
          {
               _builder = builder;
          }

          public void ConstructStandardOrder(OrderRequest dto)
          {
               _builder.Reset();
               foreach (var item in dto.Items)
               {
                    _builder.AddItem(item.ProductName, item.Price, item.Quantity);
               }
          }

          public void ConstructFullOrder(OrderRequest dto)
          {
               _builder.Reset();
               foreach (var item in dto.Items)
               {
                    _builder.AddItem(item.ProductName, item.Price, item.Quantity);
               }

               if (!string.IsNullOrEmpty(dto.DeliveryAddress))
               {
                    _builder.SetDelivery(dto.DeliveryAddress);
               }

               if (dto.WantsInstallation)
               {
                    _builder.AddInstallation();
               }

               if (dto.IsPriority)
               {
                    _builder.EnablePriority();
               }
          }


     }
}
