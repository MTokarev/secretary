using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Secretary.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetAsync();
        ValueTask<T> GetAsync(Guid id);
        EntityEntry<T> Remove(T entity);
        Task<int> SaveChangesAsync();
        T Update(T entity);
    }
}