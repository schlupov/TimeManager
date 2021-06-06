using System;
using System.Threading.Tasks;
using DAL.models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DAL.repository
{
    public class VacationRepository : GenericRepository<Vacation>
    {
        public async Task<Vacation> CreateVacationAsync(DateTime date)
        {
            Vacation vacation = new Vacation {Date = date};
            try
            {
                await InsertAsync(vacation);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return vacation;
        }
        
        public async Task<Vacation> GetVacationByDate(DateTime dateTime)
        {
            var vacations = await context.Vacation.Where(x => x.Date == dateTime).ToListAsync();
            return vacations.Count != 0 ? vacations.First() : null;
        }
        
        public async Task<bool> DeleteVacationAsync(int id)
        {
            try
            {
                await DeleteAsync(id);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        public async Task<Vacation> GetVacationByDateAsync(DateTime date)
        {
            var record = await context.Vacation.Where(x => x.Date == date).ToListAsync();
            return record.First();
        }
    }
}