using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Construieste un obiect OrderData (in-memory).
     /// Persistenta este responsabilitatea OrderRepo.
     /// </summary>
     public class OrderBuilder : IOrderBuilder
     {
          private static readonly Dictionary<DeliveryType, decimal> _deliveryCosts = new()
          {
               { DeliveryType.Standard, 0m    },
               { DeliveryType.Express,  100m  },
               { DeliveryType.SameDay,  250m  },
               { DeliveryType.Pickup,   0m    }
          };

          private const decimal InstallationCost = 500m;
          private const decimal PriorityCost     = 200m;

          private OrderData _order = new();

          public OrderBuilder() => Reset();

          public void Reset()
          {
               _order = new OrderData
               {
                    BuilderId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Status    = "New"
               };
          }

          public void SetUserId(int userId)
               => _order.UserId = userId;

          public void AddItem(int productId, string name, double price, int quantity)
          {
               _order.Items.Add(new OrderItemData
               {
                    ProductId   = productId,
                    ProductName = name,
                    Price       = price,
                    Quantity    = quantity
               });
          }

          public void SetDelivery(string address, DeliveryType deliveryType)
          {
               _order.DeliveryAddress = address;
               _order.DeliveryType    = deliveryType;
               _order.DeliveryCost    = _deliveryCosts[deliveryType];
          }

          public void AddInstallation()
               => _order.HasInstallation = true;

          public void EnablePriority()
               => _order.IsPriority = true;

          public void SetDiscount(decimal discount)
               => _order.Discount = discount < 0 ? 0 : discount;

          public void SetNotes(string notes)
               => _order.Notes = notes;

          /// <summary>
          /// Calculeaza totalul final si returneaza OrderData complet.
          /// Apelat o singura data la finalul constructiei.
          /// </summary>
          public OrderData GetResult()
          {
               decimal total = (decimal)_order.Items.Sum(i => i.Subtotal);

               total += _order.DeliveryCost;

               if (_order.HasInstallation)
                    total += InstallationCost;

               if (_order.IsPriority)
                    total += PriorityCost;

               total -= _order.Discount;

               _order.TotalPrice = total < 0 ? 0 : total;

               return _order;
          }
     }
}
