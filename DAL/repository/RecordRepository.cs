using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.models;
using Microsoft.EntityFrameworkCore;

namespace DAL.repository
{
    public class RecordRepository : GenericRepository<Work>
    {
        public async Task<Work> CreateRecordAsync(string type, DateTime inTime, DateTime outTime, string date, string comment)
        {
            Work work = new Work {Type = type, In = inTime, Out = outTime, Date = date, Comment = comment};
            try
            {
                await InsertAsync(work);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return work;
        }

        public async Task<List<Work>> ReadRecordByDateAsync(DateTime dateTime)
        {
            var desiredWork = await context.Work.Where(x => x.Date == dateTime.ToShortDateString()).ToListAsync();
            return desiredWork;
        }

        public async Task<Work> GetRecordAsync(string id)
        {
            int result;
            try
            {
                result = Int32.Parse(id);
            }
            catch (FormatException)
            {
                return null;
            }
            var desiredWork = await GetByPropertyAsync(result);
            return desiredWork;
        }

        public async Task UpdateTypeAsync(string newType, string id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.Type = newType;
                await SaveAsync();
            }
        }

        public async Task UpdateInTimeAsync(DateTime inTime, string id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.In = inTime;
                await SaveAsync();
            }
        }
        
        public async Task UpdateOutTimeAsync(DateTime outTime, string id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.Out = outTime;
                await SaveAsync();
            }
        }
        
        public async Task UpdateDateAsync(string date, string id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.Date = date;
                await SaveAsync();
            }
        }

        public async Task UpdateCommentAsync(string comment, string id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.Comment = comment;
                await SaveAsync();
            }
        }

        public async Task<bool> DeleteRecordAsync(string id)
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
        
        public async Task<bool> DeleteRecordAsync(DateTime date)
        {
            var records = await ReadRecordByDateAsync(date);
            foreach (var record in records)
            {
                try
                {
                    await DeleteAsync(record.Id);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<Work> GetRecordByDateAsync(string date)
        {
            var record = await context.Work.Where(x => x.Date == date).ToListAsync();
            return record.First();
        }
    }
}