using CH_Store.Application.Order.Interfaces;
using CH_Store.Domain.DTOs;
using CH_Store.Domain.Enums;

namespace CH_Store.Application.Order.Services
{
     /// <summary>
     /// Stie RETELE de constructie — nu stie ce builder primeste.
     /// Poate fi folosit cu OrderBuilder (produce date) sau
     /// cu OrderReportBuilder (produce raport text).
     /// </summary>
     public class OrderDirector
     {
          private readonly IOrderBuilder _builder;

          public OrderDirector(IOrderBuilder builder)
               => _builder = builder;

          // ──────────────────────────────────────────
          // Reteta 1: Comanda Standard
          // Produse + livrare standard. Fara extras.
          // ──────────────────────────────────────────
          public void ConstructStandardOrder(OrderRequest dto)
          {
               _builder.Reset();
               _builder.SetUserId(dto.UserId);

               foreach (var item in dto.Items)
                    _builder.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);

               _builder.SetDelivery(dto.DeliveryAddress, DeliveryType.Standard);

               if (!string.IsNullOrWhiteSpace(dto.Notes))
                    _builder.SetNotes(dto.Notes);
          }

          // ──────────────────────────────────────────
          // Reteta 2: Comanda Completa
          // Toate optiunile din request activate.
          // ──────────────────────────────────────────
          public void ConstructFullOrder(OrderRequest dto)
          {
               _builder.Reset();
               _builder.SetUserId(dto.UserId);

               foreach (var item in dto.Items)
                    _builder.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);

               _builder.SetDelivery(dto.DeliveryAddress, dto.DeliveryType);

               if (dto.WantsInstallation)
                    _builder.AddInstallation();

               if (dto.IsPriority)
                    _builder.EnablePriority();

               if (dto.Discount > 0)
                    _builder.SetDiscount(dto.Discount);

               if (!string.IsNullOrWhiteSpace(dto.Notes))
                    _builder.SetNotes(dto.Notes);
          }

          // ──────────────────────────────────────────
          // Reteta 3: Comanda Express
          // Forteaza livrare Express + prioritate.
          // ──────────────────────────────────────────
          public void ConstructExpressOrder(OrderRequest dto)
          {
               _builder.Reset();
               _builder.SetUserId(dto.UserId);

               foreach (var item in dto.Items)
                    _builder.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);

               _builder.SetDelivery(dto.DeliveryAddress, DeliveryType.Express);
               _builder.EnablePriority();

               if (dto.WantsInstallation)
                    _builder.AddInstallation();

               if (!string.IsNullOrWhiteSpace(dto.Notes))
                    _builder.SetNotes(dto.Notes);
          }

          // ──────────────────────────────────────────
          // Reteta 4: Comanda en-Gros (Bulk)
          // Cantitati mari => reducere aplicata automat.
          // ──────────────────────────────────────────
          public void ConstructBulkOrder(OrderRequest dto)
          {
               _builder.Reset();
               _builder.SetUserId(dto.UserId);

               foreach (var item in dto.Items)
                    _builder.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);

               _builder.SetDelivery(dto.DeliveryAddress, DeliveryType.Standard);

               // Reducere trimisa din request sau calculata automat (10% pentru bulk)
               decimal discount = dto.Discount > 0
                    ? dto.Discount
                    : (decimal)dto.Items.Sum(i => i.Price * i.Quantity) * 0.10m;

               _builder.SetDiscount(discount);

               if (!string.IsNullOrWhiteSpace(dto.Notes))
                    _builder.SetNotes(dto.Notes);
          }
     }
}
