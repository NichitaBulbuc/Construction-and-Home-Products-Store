using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.DBContext
{
     public class NotificationContext : DbContext
     {
          public NotificationContext(DbContextOptions<NotificationContext> options) : base(options) { }

          public DbSet<NotificationLog> NotificationLogs { get; set; }
     }
}
