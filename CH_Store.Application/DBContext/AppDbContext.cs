using CH_Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CH_Store.Application.DBContext
{
     public class AppDbContext : DbContext
     {
          public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

          // ─── Tabele principale ───────────────────────────────────────────────
          public DbSet<UDbTable>         Users      { get; set; }
          public DbSet<ProductDbTable>   Products   { get; set; }
          public DbSet<OrderDbTable>     Orders     { get; set; }
          public DbSet<OrderItemDbTable> OrderItems { get; set; }

          // ─── Tabele Composite Pattern (Catalog) ──────────────────────────────
          public DbSet<CatalogKitDbTable>     CatalogKits     { get; set; }
          public DbSet<CatalogKitItemDbTable> CatalogKitItems { get; set; }

          // ─── Tabele Observer Pattern (Audit log comenzi) ─────────────────────
          public DbSet<OrderEventLogDbTable> OrderEventLogs { get; set; }

          protected override void OnModelCreating(ModelBuilder modelBuilder)
          {
               base.OnModelCreating(modelBuilder);

               // ── Orders → Users ────────────────────────────────────────────────
               // Multi comenzi per user, fara cascade delete
               modelBuilder.Entity<OrderDbTable>()
                    .HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

               // ── OrderItems → Orders ───────────────────────────────────────────
               // Cascade: stergem comanda => stergem si pozitiile ei
               modelBuilder.Entity<OrderItemDbTable>()
                    .HasOne(oi => oi.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

               // ── OrderItems → Products ─────────────────────────────────────────
               // Restrict: produsul ramane in catalog chiar daca comanda e stearsa
               modelBuilder.Entity<OrderItemDbTable>()
                    .HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

               // ── CatalogKitItems → CatalogKits (parinte) ───────────────────────
               // Cascade: stergem kitul => stergem si elementele lui
               modelBuilder.Entity<CatalogKitItemDbTable>()
                    .HasOne(i => i.Kit)
                    .WithMany(k => k.Items)
                    .HasForeignKey(i => i.KitId)
                    .OnDelete(DeleteBehavior.Cascade);

               // ── CatalogKitItems → Products (Leaf) ─────────────────────────────
               // Restrict: produsul ramane in DB chiar daca e scos din kit
               modelBuilder.Entity<CatalogKitItemDbTable>()
                    .HasOne(i => i.Product)
                    .WithMany()
                    .HasForeignKey(i => i.ProductId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

               // ── CatalogKitItems → SubKit (Composite copil) ────────────────────
               // NoAction: evitam cicluri de cascade (SQL Server limiteaza cascade pe self-ref)
               modelBuilder.Entity<CatalogKitItemDbTable>()
                    .HasOne(i => i.SubKit)
                    .WithMany()
                    .HasForeignKey(i => i.SubKitId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.NoAction);

               // ── OrderEventLogs → Orders ───────────────────────────────────────
               // NoAction: stergerea unei comenzi nu sterge automat logurile ei
               modelBuilder.Entity<OrderEventLogDbTable>()
                    .HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.NoAction);
          }
     }
}
