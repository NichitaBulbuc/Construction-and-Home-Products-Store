using CH_Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.DBContext
{
     public class AppDbContext : DbContext
     {
          public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

          public DbSet<UDbTable>         Users      { get; set; }
          public DbSet<ProductDbTable>   Products   { get; set; }
          public DbSet<OrderDbTable>     Orders     { get; set; }
          public DbSet<OrderItemDbTable> OrderItems { get; set; }

          protected override void OnModelCreating(ModelBuilder modelBuilder)
          {
               base.OnModelCreating(modelBuilder);

               // Orders → Users (multi comenzi per user, fara cascade delete)
               modelBuilder.Entity<OrderDbTable>()
                    .HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

               // OrderItems → Orders (cascade: stergem comanda => stergem si produsele ei)
               modelBuilder.Entity<OrderItemDbTable>()
                    .HasOne(oi => oi.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

               // OrderItems → Products (fara cascade delete — produsul ramane in catalog)
               modelBuilder.Entity<OrderItemDbTable>()
                    .HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
          }
     }
}
