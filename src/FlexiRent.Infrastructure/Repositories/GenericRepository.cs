using FlexiRent.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FlexiRent.Infrastructure.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync(int skip = 0, int take = 50);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, int skip = 0, int take = 50);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }

        public virtual async Task AddAsync(T entity) { _dbSet.Add(entity); await _db.SaveChangesAsync(); }
        public virtual async Task DeleteAsync(T entity) { _dbSet.Remove(entity); await _db.SaveChangesAsync(); }
        public virtual async Task<T?> GetAsync(Guid id) => await _dbSet.FindAsync(id);
        public virtual async Task<IEnumerable<T>> GetAllAsync(int skip = 0, int take = 50) =>
            await _dbSet.Skip(skip).Take(take).ToListAsync();
        public virtual async Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, int skip = 0, int take = 50) =>
            await _dbSet.Where(predicate).Skip(skip).Take(take).ToListAsync();
        public virtual async Task UpdateAsync(T entity) { _dbSet.Update(entity); await _db.SaveChangesAsync(); }
        public virtual async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);
    }
}
