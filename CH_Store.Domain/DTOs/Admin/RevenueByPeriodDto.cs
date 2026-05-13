namespace CH_Store.Domain.DTOs.Admin
{
     public class RevenueByPeriodDto
     {
          public string  Period     { get; set; } = "";   // "2026-05-14"
          public decimal Revenue    { get; set; }
          public int     OrderCount { get; set; }
     }
}
