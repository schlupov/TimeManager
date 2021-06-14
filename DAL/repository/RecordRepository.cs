using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.models;

namespace DAL.repository
{
    public class RecordRepository : GenericRepository<Work>
    {
        public async Task<Work> CreateRecordAsync(WorkType type, string inTime, string outTime, string date,
            string comment)
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

        public List<Work> ReadRecordByDate(string dateTime, string email)
        {
            var desiredWork = context.Work.Where(x => x.UserEmail == email && x.Date == dateTime).ToList();
            return desiredWork;
        }

        public async Task<Work> GetRecordAsync(int id)
        {
            var desiredWork = await GetByPropertyAsync(id);
            return desiredWork;
        }

        public bool CheckUserRecord(string email, int id)
        {
            var desiredWork = context.Work.Where(x => x.Id == id && x.UserEmail == email).ToList();
            return desiredWork.Count != 0;
        }

        public async Task UpdateTypeAsync(WorkType newType, int id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.Type = newType;
                await SaveAsync();
            }
        }

        public async Task UpdateInTimeAsync(string inTime, int id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.In = inTime;
                await SaveAsync();
            }
        }

        public async Task UpdateOutTimeAsync(string outTime, int id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.Out = outTime;
                await SaveAsync();
            }
        }

        public async Task UpdateDateAsync(string date, int id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.Date = date;
                await SaveAsync();
            }
        }

        public async Task UpdateCommentAsync(string comment, int id)
        {
            var work = await GetRecordAsync(id);
            if (work != null)
            {
                work.Comment = comment;
                await SaveAsync();
            }
        }

        public async Task<bool> DeleteRecordAsync(int id)
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

        public async Task<bool> DeleteRecordAsync(DateTime date, string email)
        {
            var records = ReadRecordByDate(date.ToShortDateString(), email);
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
    }
}