namespace CH_Store.Domain.DTOs
{
     /// <summary>
     /// Rezultat generic pentru operatii pe comenzi (cancel, update status).
     /// </summary>
     public class OrderOperationResult
     {
          public bool   Success          { get; set; }
          public string Message          { get; set; } = "";
          public int    OrderId          { get; set; }
          public string NewStatus        { get; set; } = "";
          public bool   NotificationSent { get; set; }
     }
}
