using CH_Store.Application.Order.Interfaces;
using CH_Store.Application.Order.Strategy.Delivery;
using CH_Store.Application.Order.Strategy.Payment;
using CH_Store.Domain.Enums;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Construieste un obiect OrderData (in-memory).
     /// Persistenta este responsabilitatea OrderRepo.
     ///
     /// Strategy Pattern integrat:
     ///   • SetDelivery()         → rezolva IDeliveryStrategy via DeliveryStrategyResolver
     ///   • SetDeliveryStrategy() → primeste strategia direct (custom / override)
     ///   • SetPaymentStrategy()  → ataseaza strategia de plata aleasa de Facade
     ///
     /// Dictionarul hardcodat _deliveryCosts a fost inlocuit cu DeliveryStrategyResolver:
     ///   INAINTE: _deliveryCosts[deliveryType]  → decimal
     ///   ACUM   : DeliveryStrategyResolver.Resolve(deliveryType).CalculateCost(order)
     /// </summary>
     public class OrderBuilder : IOrderBuilder
     {
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

          /// <summary>
          /// Rezolva strategia de livrare prin DeliveryStrategyResolver si o aplica.
          /// Inlocuieste logica hardcodata _deliveryCosts[deliveryType].
          /// </summary>
          public void SetDelivery(string address, DeliveryType deliveryType)
          {
               var strategy = DeliveryStrategyResolver.Resolve(deliveryType);
               ApplyDeliveryStrategy(address, strategy);
          }

          /// <summary>
          /// Primeste o strategie de livrare injectata explicit.
          /// Util pentru strategii custom (ex: livrare gratuita pentru comenzi > 5000 MDL).
          /// </summary>
          public void SetDeliveryStrategy(string address, IDeliveryStrategy strategy)
               => ApplyDeliveryStrategy(address, strategy);

          /// <summary>
          /// Ataseaza descrierea strategiei de plata in OrderData.
          /// Apelat de OrderFacade dupa ce strategia a fost rezolvata prin PaymentStrategyResolver.
          /// </summary>
          public void SetPaymentStrategy(IPaymentStrategy strategy)
               => _order.PaymentStrategyDescription = strategy.GetDescription();

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

          // ── Helper privat ────────────────────────────────────────────────────

          private void ApplyDeliveryStrategy(string address, IDeliveryStrategy strategy)
          {
               _order.DeliveryAddress             = address;
               _order.DeliveryType                = strategy.DeliveryType;
               _order.DeliveryCost                = strategy.CalculateCost(_order);
               _order.DeliveryStrategyDescription = strategy.GetDescription();
               _order.EstimatedDeliveryDays       = strategy.GetEstimatedDays();
          }
     }
}
