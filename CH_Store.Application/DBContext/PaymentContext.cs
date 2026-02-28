using CH_Store.Application.Payments.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.DBContext
{
     public class PaymentContext: DbContext
     {
          public DbSet<PaymentData> Transactions { get; set; }

          public PaymentContext(DbContextOptions<PaymentContext> options)
               :base(options)
          {
               
          }

          protected override void OnModelCreating(ModelBuilder modelBuilder)
          {
               // Configurăm Enum-ul să fie salvat ca String în baza de date pentru lizibilitate
               modelBuilder.Entity<PaymentData>()
                   .Property(t => t.Method)
                   .HasConversion<string>();
          }
     }
}
