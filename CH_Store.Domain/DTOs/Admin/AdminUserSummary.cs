using CH_Store.Domain.Enums;

namespace CH_Store.Domain.DTOs.Admin
{
     public class AdminUserSummary
     {
          public int      Id                    { get; set; }
          public string   Username              { get; set; } = "";
          public string   Email                 { get; set; } = "";
          public string?  Phone                 { get; set; }
          public string?  Address               { get; set; }
          public URole    Level                 { get; set; }
          public string   LevelLabel            => Level.ToString();
          public DateTime LastLoginDateTime     { get; set; }
          public DateTime RegistartionDateTime  { get; set; }
          public int      TotalOrders           { get; set; }
          public decimal  TotalSpent            { get; set; }
     }
}
