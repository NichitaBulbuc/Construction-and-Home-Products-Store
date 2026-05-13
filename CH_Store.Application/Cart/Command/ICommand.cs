namespace CH_Store.Application.Cart.Command
{
     /// <summary>
     /// Command Pattern — interfata de baza pentru toate comenzile cosului.
     ///
     /// Fiecare actiune a utilizatorului (adauga, sterge, modifica, goleste cosul)
     /// devine un obiect ICommand cu Execute + Undo.
     ///
     /// Beneficii fata de apeluri directe de serviciu:
     ///   • Undo  — anularea ultimei operatii fara logica speciala in Controller
     ///   • History — jurnal complet de actiuni per user in IMemoryCache
     ///   • Retry  — re-executia comenzii esecute (apel command.ExecuteAsync() din nou)
     ///   • Queue  — comenzile pot fi puse intr-o coada si procesate asincron
     /// </summary>
     public interface ICommand
     {
          /// <summary>Identificator scurt al tipului de comanda (ex: "AddItemToCart").</summary>
          string CommandName { get; }

          /// <summary>Descriere lizibila a operatiei (ex: "Adaugat 'Ciment' x2 in cos").</summary>
          string Description { get; }

          /// <summary>Momentul crearii comenzii.</summary>
          DateTime OccurredAt { get; }

          /// <summary>Executa operatia encapsulata.</summary>
          Task ExecuteAsync();

          /// <summary>
          /// Anuleaza operatia, restaurand starea anterioara.
          /// Poate fi apelat o singura data dupa ExecuteAsync.
          /// </summary>
          Task UndoAsync();
     }
}
