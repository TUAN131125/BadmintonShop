using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Cập nhật signature hàm GetAllAsync để khớp với Class (thêm orderBy)
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "");

        // Đổi int -> object để hỗ trợ cả Guid hoặc String ID
        Task<T> GetByIdAsync(object id);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        // Thêm hàm xóa theo ID (vì trong Class đã có)
        Task DeleteAsync(object id);

        // Hàm hỗ trợ Query Service
        IQueryable<T> GetQuery();
    }
}