using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.DBContext
{
     public class OrderContext: DbContext
     {
          public OrderContext(DbContextOptions<OrderContext> options): base(options) 
          { 
          }

          public DbSet<OrderData> Orders { get; set; }
          public DbSet<OrderItemData> OrderItems { get; set; }

          protected override void OnModelCreating(ModelBuilder modelBuilder)
          {
               modelBuilder.Entity<OrderData>()
                   .HasKey(o => o.Id);

               base.OnModelCreating(modelBuilder);
          }
     }
}
