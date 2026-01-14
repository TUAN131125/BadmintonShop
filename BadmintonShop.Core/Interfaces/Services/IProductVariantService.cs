using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BadmintonShop.Core.Entities;

namespace BadmintonShop.Core.Interfaces.Services
{
    public interface IProductVariantService
    {
        Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId);
        Task<ProductVariant> GetByIdAsync(int id);
        Task<IEnumerable<ProductVariant>> GetAllAsync();

        Task CreateAsync(ProductVariant variant);
        Task UpdateAsync(ProductVariant variant);
        Task SoftDeleteAsync(int id);
    }
}
