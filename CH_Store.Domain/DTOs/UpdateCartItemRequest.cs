namespace CH_Store.Domain.DTOs
{
     /// <summary>Cerere de actualizare cantitate produs in cos.</summary>
     public class UpdateCartItemRequest
     {
          /// <summary>Noua cantitate absoluta (nu delta). 0 = sterge produsul din cos.</summary>
          public int Quantity { get; set; }
     }
}
