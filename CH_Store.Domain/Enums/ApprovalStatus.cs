namespace CH_Store.Domain.Enums
{
     /// <summary>
     /// Rezultatul posibil al lantului de aprobare a comenzii.
     /// Severitate crescatoare: Approved < PendingManagerApproval < Rejected
     /// </summary>
     public enum ApprovalStatus
     {
          /// <summary>Comanda a trecut toate verificarile — se proceseaza imediat.</summary>
          Approved = 0,

          /// <summary>
          /// Comanda ridica un semnal de atentie (discount mare, valoare B2B, activitate suspecta).
          /// Salvata cu status "PendingApproval" — un manager trebuie sa o aprobe manual.
          /// </summary>
          PendingManagerApproval = 1,

          /// <summary>
          /// Comanda respinsa (stoc insuficient, produs inexistent).
          /// Nu este salvata in baza de date.
          /// </summary>
          Rejected = 2
     }
}
