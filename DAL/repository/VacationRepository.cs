using System;
using System.Threading.Tasks;
using DAL.models;
using System.Linq;

namespace DAL.repository
{
    public class VacationRepository : GenericRepository<Vacation>
    {
        public async Task<Vacation> CreateVacationAsync(string date)
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

        public Vacation GetVacationByDate(string date, string email)
        {
            var desiredVacation = context.Vacation.Where(x => x.UserEmail == email && x.Date == date).ToList();

            try
            {
                return desiredVacation.First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}