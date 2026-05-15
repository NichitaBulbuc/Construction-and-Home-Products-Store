using CH_Store.Domain.Models;

namespace CH_Store.Domain.DTOs
{
     public class CloneOrderResponse
     {
          /// <summary>ID-ul noii comenzi salvate in DB.</summary>
          public int  DbId            { get; set; }

          /// <summary>ID-ul comenzii-template din care s-a clonat.</summary>
          public int  TemplateOrderId { get; set; }

          public Guid BuilderId       { get; set; }

          /// <summary>Totalul din comanda-template (preturi vechi).</summary>
          public decimal OldTotal     { get; set; }

          /// <summary>Totalul noii comenzi (preturi curente din DB).</summary>
          public decimal NewTotal     { get; set; }

          /// <summary>Diferenta absoluta intre totalul nou si cel vechi.</summary>
          public decimal Difference   => NewTotal - OldTotal;

          /// <summary>Diferenta procentuala.</summary>
          public double DifferencePercent =>
               OldTotal > 0 ? (double)((NewTotal - OldTotal) / OldTotal * 100) : 0;

          /// <summary>Lista produselor cu preturile vechi si noi.</summary>
          public List<ItemPriceUpdate> PriceUpdates { get; set; } = new();

          /// <summary>Numarul de produse cu pret modificat.</summary>
          public int ChangedPricesCount => PriceUpdates.Count(p => p.PriceChanged);

          /// <summary>Devizul estimativ complet (text).</summary>
          public string Deviz { get; set; } = "";
     }
}
