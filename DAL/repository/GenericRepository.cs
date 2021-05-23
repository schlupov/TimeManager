using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DAL.repository
{
    public abstract class GenericRepository<T> where T : class
    {
        protected readonly TimeManagerDbContext context;
        private readonly DbSet<T> table;

        protected GenericRepository()
        {
            context = new TimeManagerDbContext();
            table = context.Set<T>();
        }

        protected async Task<IEnumerable<T>> GetAllAsync()
        {
            return await table.ToListAsync();
        }

        protected async Task<T> GetByEmailAsync(object property)
        {
            return await table.FindAsync(property);
        }

        protected async Task InsertAsync(T obj)
        {
            await table.AddAsync(obj);
            await SaveAsync();
        }

        protected async Task DeleteAsync(object id)
        {
            T existing = await table.FindAsync(id);
            table.Remove(existing ?? throw new InvalidOperationException());
            await SaveAsync();
        }

        protected async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}