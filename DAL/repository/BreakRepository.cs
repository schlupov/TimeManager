using System;
using System.Threading.Tasks;
using DAL.models;
using System.Linq;

namespace DAL.repository
{
    public class BreakRepository : GenericRepository<Break>
    {
        public Break CreateBreak(BreakType breakType, string inTime, string outTime, string date)
        {
            Break workBreak = new Break {Type = breakType, In = inTime, Out = outTime, Date = date};
            return workBreak;
        }

        public async Task<Break> UpdateBreakAsync(int workBreakId, BreakType breakTypeEnum, string inTimeBreak,
            string outTimeBreak)
        {
            var desiredBreak = context.Break.First(x => x.Id == workBreakId);
            desiredBreak.Type = breakTypeEnum;
            desiredBreak.In = inTimeBreak;
            desiredBreak.Out = outTimeBreak;
            await context.SaveChangesAsync();
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

        public async Task<Break> DeleteBreakAsync(string date, string email)
        {
            var bBreak = ReadBreakByDate(date, email);
            try
            {
                var breakFromDb = context.Break.First(x => x.Id == bBreak.Id);
                context.Break.Remove(breakFromDb);
                await context.SaveChangesAsync();
                return breakFromDb;
            }
            catch (NullReferenceException)
            {
                await Console.Error.WriteLineAsync("You don't have a break on this day");
            }

            return null;
        }
    }
}