using System;
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

        protected async Task<T> GetByPropertyAsync(object property)
        {
            return await table.FindAsync(property);
        }

        protected async Task InsertAsync(T obj)
        {
            await table.AddAsync(obj);
            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}