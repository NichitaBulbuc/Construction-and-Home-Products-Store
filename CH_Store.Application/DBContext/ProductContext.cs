using CH_Store.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.DBContext
{
     public class ProductContext: DbContext
     {
          public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }

          public DbSet<ProductPrototypeData> Products { get; set; }
     }
}
