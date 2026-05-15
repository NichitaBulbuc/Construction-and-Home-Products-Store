namespace CH_Store.Domain.DTOs.Admin
{
     public class AdminOrderFilterRequest
     {
          public string?   Status    { get; set; }
          public int?      UserId    { get; set; }
          public DateTime? DateFrom  { get; set; }
          public DateTime? DateTo    { get; set; }
          public decimal?  MinTotal  { get; set; }
          public decimal?  MaxTotal  { get; set; }

          public int    Page     { get; set; } = 1;
          public int    PageSize { get; set; } = 20;
          public string SortBy   { get; set; } = "OrderDate";
          public bool   SortDesc { get; set; } = true;
     }
}
