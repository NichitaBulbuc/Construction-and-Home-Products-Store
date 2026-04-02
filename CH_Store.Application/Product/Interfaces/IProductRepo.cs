using CH_Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH_Store.Application.Product.Interfaces
{
     public interface IProductRepo
     {
          Task<ProductPrototypeData?> GetByIdAsync(int id);
          Task<IEnumerable<ProductPrototypeData>> GetAllAsync();
     }
}
