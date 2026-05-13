using CH_Store.Domain.Enums;

namespace CH_Store.Application.Order.Interfaces
{
     public interface IOrderBuilder
     {
          void Reset();

          /// <summary>Adauga un produs in comanda (productId referenciat din ProductDbTable).</summary>
          void AddItem(int productId, string name, double price, int quantity);

          /// <summary>Seteaza adresa si tipul de livrare. Costul se calculeaza automat.</summary>
          void SetDelivery(string address, DeliveryType deliveryType);

          /// <summary>Include serviciul de montaj/instalare (+500 MDL).</summary>
          void AddInstallation();

          /// <summary>Activeaza procesarea prioritara (+200 MDL).</summary>
          void EnablePriority();

          /// <summary>Aplica o reducere fixa pe total comanda.</summary>
          void SetDiscount(decimal discount);

          /// <summary>Observatii/mentiuni speciale pentru comanda.</summary>
          void SetNotes(string notes);

          /// <summary>Asociaza comanda cu un user din UDbTable.</summary>
          void SetUserId(int userId);
     }
}
