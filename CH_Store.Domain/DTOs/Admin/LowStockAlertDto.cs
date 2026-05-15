namespace CH_Store.Domain.DTOs.Admin
{
     public class LowStockAlertDto
     {
          public int    ProductId     { get; set; }
          public string ProductName   { get; set; } = "";
          public string Category      { get; set; } = "";
          public int    StockQuantity { get; set; }
          public double Price         { get; set; }
          public string Severity      => StockQuantity == 0 ? "OutOfStock" : "LowStock";
     }
}
