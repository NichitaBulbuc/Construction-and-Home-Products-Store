namespace CH_Store.Domain.DTOs
{
     public class CloneOrderRequest
     {
          /// <summary>User-ul care emite noul deviz (poate fi diferit de autorul template-ului).</summary>
          public int UserId { get; set; }

          /// <summary>
          /// Ajustari de cantitate: ProductId → cantitate noua.
          /// Produsele neincluse pastreaza cantitatea din template.
          /// </summary>
          public Dictionary<int, int> QuantityOverrides { get; set; } = new();

          /// <summary>Adresa de livrare pentru noua comanda. Null = se preia din template.</summary>
          public string? NewDeliveryAddress { get; set; }

          /// <summary>Observatii specifice noului deviz.</summary>
          public string? Notes { get; set; }
     }
}
