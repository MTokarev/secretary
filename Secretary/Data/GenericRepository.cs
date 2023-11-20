using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Secretary.Interfaces;

namespace Secretary.Data
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DataContext _context;

        public GenericRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAsync()
            => await _context
                .Set<T>()
                .ToListAsync();
    
        public async Task<IEnumerable<T>> GetAsync<TKey>(Expression<Func<T, bool>> expression, Expression<Func<T, TKey>> orderBy, int take, int skip)
            => await _context
                .Set<T>()
                .Where(expression)
                .OrderByDescending(orderBy)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        public ValueTask<T> GetAsync(Guid id)
            => _context
                .Set<T>()
                .FindAsync(id);

        public async Task<T> CreateAsync(T entity)
            => (await _context.AddAsync(entity)).Entity;

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public EntityEntry<T> Remove(T entity)
            => _context.Remove(entity);

        public T Update(T entity)
            => _context.Update(entity).Entity;

        public async Task<int> GetCount(Expression<Func<T, bool>> expression)
        {
            return await _context
                .Set<T>()
                .CountAsync(expression);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
            => await _context.Set<T>()
                .Where(expression)
                .ToListAsync();
    }
}

