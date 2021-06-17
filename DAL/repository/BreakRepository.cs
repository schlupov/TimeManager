using System;
using System.Threading.Tasks;
using DAL.models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DAL.repository
{
    public class BreakRepository : GenericRepository<Break>
    {
        public async Task<Break> CreateBreakAsync(BreakType breakType, string inTime, string outTime, string date)
        {
            Break workBreak = new Break {Type = breakType, In = inTime, Out = outTime, Date = date};
            try
            {
                await InsertAsync(workBreak);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return workBreak;
        }

        private async Task<Break> GetBreakAsync(int id)
        {
            var desiredBreak = await GetByPropertyAsync(id);
            return desiredBreak;
        }

        public async Task<Break> UpdateBreakAsync(int workBreakId, BreakType breakTypeEnum, string inTimeBreak,
            string outTimeBreak)
        {
            var desiredBreak = await GetBreakAsync(workBreakId);
            desiredBreak.Type = breakTypeEnum;
            desiredBreak.In = inTimeBreak;
            desiredBreak.Out = outTimeBreak;
            await SaveAsync();
            return desiredBreak;
        }

        public Break ReadBreakByDate(string date, string email)
        {
            var desiredBreak = context.Break.Where(x => x.UserEmail == email && x.Date == date).ToList();
            try
            {
                return desiredBreak.First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public async Task DeleteBreakAsync(string date, string email)
        {
            var bBreak = ReadBreakByDate(date, email);
            try
            {
                await DeleteAsync(bBreak.Id);
            }
            catch (NullReferenceException)
            {
                await Console.Error.WriteLineAsync("You don't have a break on this day");
            }
        }
    }
}