namespace CH_Store.Domain.DTOs.Admin
{
     public class AdminOrderSummary
     {
          public int      Id           { get; set; }
          public int      UserId       { get; set; }
          public string?  Username     { get; set; }
          public DateTime OrderDate    { get; set; }
          public decimal  TotalAmount  { get; set; }
          public string   Status       { get; set; } = "";
          public string   DeliveryType { get; set; } = "";
          public int      ItemCount    { get; set; }
          public bool     HasInstallation { get; set; }
          public bool     IsPriority   { get; set; }
          public decimal  Discount     { get; set; }
          public string?  Notes        { get; set; }
     }
}
