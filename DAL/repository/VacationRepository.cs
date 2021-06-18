using System;
using System.Threading.Tasks;
using DAL.models;
using System.Linq;

namespace DAL.repository
{
    public class VacationRepository : GenericRepository<Vacation>
    {
        public Vacation CreateVacation(string date)
        {
            Vacation vacation = new Vacation {Date = date};
            return vacation;
        }

        public async Task<Vacation> DeleteVacationAsync(int id)
        {
            try
            {
                var vacationFromDb = context.Vacation.First(x => x.Id == id);
                context.Vacation.Remove(vacationFromDb);
                await context.SaveChangesAsync();
                return vacationFromDb;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
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