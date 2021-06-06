using System;
using System.Threading.Tasks;
using DAL.models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DAL.repository
{
    public class BreakRepository : GenericRepository<Break>
    {
        public async Task<Break> CreateBreakAsync(BreakType breakType, DateTime inTime, DateTime outTime)
        {
            Break workBreak = new Break {Type = breakType, In = inTime, Out = outTime};
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

        public async Task<Break> UpdateBreakAsync(int workBreakId, BreakType breakTypeEnum, DateTime inTimeBreak, DateTime outTimeBreak)
        {
            var desiredBreak = await GetBreakAsync(workBreakId);
            desiredBreak.Type = breakTypeEnum;
            desiredBreak.In = inTimeBreak;
            desiredBreak.Out = outTimeBreak;
            await SaveAsync();
            return desiredBreak;
        }
    }
}