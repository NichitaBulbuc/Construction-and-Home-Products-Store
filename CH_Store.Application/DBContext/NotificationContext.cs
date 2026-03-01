using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.DBContext
{
     public class NotificationContext: DbContext
     {
          public NotificationContext(DbContextOptions<NotificationContext> options) : base(options) { }

          public DbSet<OrderData> Orders { get; set; }
     }
}
