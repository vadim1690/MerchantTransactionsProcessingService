using MerchantTransactionProcessing.Data.Entities;
using MerchantTransactionProcessing.Models;
using System.Linq.Expressions;

namespace MerchantTransactionProcessing.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid Id);
        Task<T?> GetByIdAsync(Guid Id,params Expression<Func<T, object>>[] includes);
        Task<IReadOnlyList<T>> ListAllAsync();

        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        void Delete(T entity);

        Task<IReadOnlyList<T>> FilterAsync(params Expression<Func<T, bool>>[] filters);
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize,params Expression<Func<T, bool>>[] filters);
    }
}
