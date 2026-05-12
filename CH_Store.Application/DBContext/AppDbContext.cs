using CH_Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.DBContext
{
     public class AppDbContext : DbContext
     {
          public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

          public DbSet<UDbTable> Users { get; set; }
          public DbSet<ProductDbTable> Products { get; set; }
          public DbSet<OrderDbTable> Orders { get; set; }

          protected override void OnModelCreating(ModelBuilder modelBuilder)
          {
               base.OnModelCreating(modelBuilder);

               modelBuilder.Entity<OrderDbTable>()
                    .HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
          }
     }
}
