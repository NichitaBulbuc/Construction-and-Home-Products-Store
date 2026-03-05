using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Order.Services
{
     public class OrderBuilder : IOrderBuilder
     {
          private OrderData _order = new OrderData();

          public OrderBuilder()
          {
               Reset();
          }

          public void Reset()
          {
               _order = new OrderData();
               _order.Id = Guid.NewGuid();
          }

        
          public void AddItem(string name, double price, int quantity)
          {
               _order.Items.Add(new OrderItemData
               {
                    ProductName = name,
                    Price = price,
                    Quantity = quantity
               });
          }

          public void SetDelivery(string address)
          {
               _order.DeliveryAddress = address;
          }

          public void AddInstallation()
          {
               _order.HasInstallation = true;
          }

          public void EnablePriority()
          {
               _order.IsPriority = true;
          }

          public OrderData GetResult()
          {
               double total = _order.Items.Sum(i => i.Price * i.Quantity);

               if (_order.HasInstallation)
                    total += 500;

               if (_order.IsPriority)
                    total += 200;

               _order.TotalPrice = total;

               return _order;
          }

          
     }
}
