using CH_Store.Domain.Models;

namespace CH_Store.Application.Order.Interfaces
{
     /// <summary>
     /// Prototype Pattern — interfata de clonare pentru comenzi.
     ///
     /// Analogie cu ProductPrototype din modulul Product, dar aplicata comenzilor:
     ///   ProductPrototype → cloneaza sabloane de produs (blueprint)
     ///   IOrderPrototype  → cloneaza comenzi complete (template de deviz)
     /// </summary>
     public interface IOrderPrototype
     {
          /// <summary>Cloneaza comanda-template pastrand cantitatile originale.</summary>
          OrderData Clone();

          /// <summary>
          /// Cloneaza comanda-template si ajusteaza cantitatile specificate.
          /// Produsele neincluse in <paramref name="quantityOverrides"/> isi pastreaza cantitatea.
          /// </summary>
          OrderData CloneWithAdjustments(Dictionary<int, int> quantityOverrides);
     }
}
