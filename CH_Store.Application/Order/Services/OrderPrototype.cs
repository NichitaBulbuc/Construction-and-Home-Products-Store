using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Concrete Prototype — produce copii profunde (deep copy) ale unei comenzi-template.
     ///
     /// IMPORTANT: Clona contine preturile VECHI din template.
     /// Actualizarea preturilor la valorile curente din DB este responsabilitatea
     /// OrderTemplateService (pasul de "refresh prices" dupa clonare).
     /// </summary>
     public class OrderPrototype : IOrderPrototype
     {
          private readonly OrderData _template;

          public OrderPrototype(OrderData template)
          {
               _template = template;
          }

          /// <inheritdoc/>
          public OrderData Clone()
          {
               return new OrderData
               {
                    BuilderId       = Guid.NewGuid(),      // ID nou — comanda distincta
                    UserId          = _template.UserId,
                    DeliveryAddress = _template.DeliveryAddress,
                    DeliveryType    = _template.DeliveryType,
                    DeliveryCost    = _template.DeliveryCost,
                    HasInstallation = _template.HasInstallation,
                    IsPriority      = _template.IsPriority,
                    Discount        = _template.Discount,
                    Notes           = _template.Notes,
                    Status          = "New",
                    CreatedAt       = DateTime.UtcNow,

                    // Deep copy al listei de produse
                    Items = _template.Items.Select(i => new OrderItemData
                    {
                         ProductId   = i.ProductId,
                         ProductName = i.ProductName,
                         Price       = i.Price,     // pret vechi — va fi refreshed de service
                         Quantity    = i.Quantity
                    }).ToList()
               };
          }

          /// <inheritdoc/>
          public OrderData CloneWithAdjustments(Dictionary<int, int> quantityOverrides)
          {
               var clone = Clone();

               foreach (var item in clone.Items)
               {
                    if (quantityOverrides.TryGetValue(item.ProductId, out int newQty) && newQty > 0)
                    {
                         item.Quantity = newQty;
                    }
               }

               return clone;
          }
     }
}
