namespace CH_Store.Domain.DTOs.Admin
{
     public class AdminDashboardStats
     {
          // ── Comenzi ───────────────────────────────────────────────────────────
          public int     TotalOrders        { get; set; }
          public int     OrdersToday        { get; set; }
          public int     PendingApproval    { get; set; }
          public int     InProgress         { get; set; }   // Processing + Shipped
          public int     DeliveredToday     { get; set; }
          public int     CancelledTotal     { get; set; }

          // ── Venituri ─────────────────────────────────────────────────────────
          public decimal TotalRevenue       { get; set; }
          public decimal RevenueToday       { get; set; }
          public decimal RevenueThisMonth   { get; set; }

          // ── Produse ──────────────────────────────────────────────────────────
          public int     TotalProducts      { get; set; }
          public int     OutOfStockProducts { get; set; }
          public int     LowStockProducts   { get; set; }

          // ── Utilizatori ──────────────────────────────────────────────────────
          public int     TotalUsers         { get; set; }
          public int     NewUsersThisMonth  { get; set; }

          // ── Liste detaliate ───────────────────────────────────────────────────
          public List<OrdersByStatusEntry>  OrdersByStatus  { get; set; } = new();
          public List<RevenueByPeriodDto>   RevenueLastDays { get; set; } = new();
          public List<LowStockAlertDto>     StockAlerts     { get; set; } = new();
          public List<RecentActivityEntry>  RecentActivity  { get; set; } = new();
     }

     public class OrdersByStatusEntry
     {
          public string Status { get; set; } = "";
          public int    Count  { get; set; }
          public decimal TotalValue { get; set; }
     }

     public class RecentActivityEntry
     {
          public int      OrderId   { get; set; }
          public int      UserId    { get; set; }
          public string   OldStatus { get; set; } = "";
          public string   NewStatus { get; set; } = "";
          public decimal  Total     { get; set; }
          public DateTime OccurredAt { get; set; }
          public string?  Notes     { get; set; }
     }
}
