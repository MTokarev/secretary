using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Secretary.Interfaces
{
    /// <summary>
    /// Represent interface for generic repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenericRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetAsync();
        
        /// <summary>
        /// Returns a list of items
        /// Can be used to return paged data
        /// </summary>
        /// <param name="expression">Condition</param>
        /// <param name="orderBy">OrderBy property</param>
        /// <param name="take">How many elements to take</param>
        /// <param name="skip">How many elements to skip</param>
        /// <typeparam name="TKey">Property to orderBy on</typeparam>
        /// <returns>A list of items</returns>
        Task<IEnumerable<T>> GetAsync<TKey>(Expression<Func<T, bool>> expression, Expression<Func<T, TKey>> orderBy, int take, int skip);
        ValueTask<T> GetAsync(Guid id);
        EntityEntry<T> Remove(T entity);
        Task<int> SaveChangesAsync();
        T Update(T entity);
        
        /// <summary>
        /// Get element counts based on the expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>The number of elements</returns>
        Task<int> GetCount(Expression<Func<T, bool>> expression);
    }
}