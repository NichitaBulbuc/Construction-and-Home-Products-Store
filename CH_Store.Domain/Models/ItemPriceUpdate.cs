namespace CH_Store.Domain.Models
{
     /// <summary>
     /// Inregistreaza diferenta de pret pentru un produs in momentul clonarii comenzii.
     /// Folosit de DevizGenerator pentru a afisa comparatia vechi → nou.
     /// </summary>
     public class ItemPriceUpdate
     {
          public int    ProductId   { get; set; }
          public string ProductName { get; set; } = "";
          public int    Quantity    { get; set; }

          /// <summary>Pretul din comanda-template (poate fi invechit).</summary>
          public double OldPrice { get; set; }

          /// <summary>Pretul curent din ProductDbTable (SQL Server).</summary>
          public double NewPrice { get; set; }

          /// <summary>True daca pretul s-a schimbat fata de template.</summary>
          public bool   PriceChanged  => Math.Abs(NewPrice - OldPrice) > 0.001;

          /// <summary>Diferenta per unitate (pozitiv = scumpire, negativ = ieftinire).</summary>
          public double UnitDifference => NewPrice - OldPrice;

          public double OldSubtotal => OldPrice * Quantity;
          public double NewSubtotal => NewPrice * Quantity;

          /// <summary>Diferenta totala pentru aceasta linie.</summary>
          public double LineDifference => NewSubtotal - OldSubtotal;

          /// <summary>True daca pretul nu a putut fi gasit in DB (produs sters/inexistent).</summary>
          public bool   PriceNotFound { get; set; }
     }
}
