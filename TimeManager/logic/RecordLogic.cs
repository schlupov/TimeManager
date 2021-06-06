using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DAL;
using DAL.models;
using DAL.repository;

namespace TimeManager.logic
{
    public class RecordLogic
    {
        private readonly RecordRepository recordRepository = new();
        private readonly BreakRepository breakRepository = new();
        private readonly VacationRepository vacationRepository = new();

        public async Task<bool> CreateRecordAsync(string type, DateTime inTime, DateTime outTime, string date, string comment)
        {
            var work = await recordRepository.CreateRecordAsync(type, inTime, outTime, date, comment);
            return work == null;
        }

        public async Task<List<Work>> ReadRecordByDateAsync(DateTime dateTime)
        {
            var work = await recordRepository.ReadRecordByDateAsync(dateTime);
            return work;
        }

        public async Task<Work> GetRecordAsync(string id)
        {
            var work = await recordRepository.GetRecordAsync(id);
            return work;
        }

        public async Task UpdateTypeAsync(string newType, string id)
        {
            if (newType != "bugzilla" && newType != "issue" && newType != "documentation" &&
                newType != "release")
            {
                await Console.Error.WriteLineAsync("Couldn't update the record");
                return;
            }
            await recordRepository.UpdateTypeAsync(newType, id);
        }

        public async Task UpdateInTimeAsync(string inTimeString, string id)
        {
            var inTime = DateTime.Parse(inTimeString, new CultureInfo("cs-CZ"));
            await recordRepository.UpdateInTimeAsync(inTime, id);
        }

        public async Task UpdateDateAsync(string date, string id)
        {
            var dateTime = DateTime.Parse(date, new CultureInfo("cs-CZ"));
            await recordRepository.UpdateDateAsync(dateTime.ToShortDateString(), id);
        }

        public async Task UpdateOutTimeAsync(string outTimeString, string id)
        {
            var outTime = DateTime.Parse(outTimeString, new CultureInfo("cs-CZ"));
            await recordRepository.UpdateOutTimeAsync(outTime, id);
        }

        public async Task UpdateCommentAsync(string comment, string id)
        {
            if (comment is {Length: > 200})
            {
                await Console.Error.WriteLineAsync("Comment is too long");
                return;
            }
            await recordRepository.UpdateCommentAsync(comment, id);
        }

        public async Task DeleteRecordAsync(string id)
        {
            var success = await recordRepository.DeleteRecordAsync(id);
            if (!success)
            {
                await Console.Error.WriteLineAsync("Couldn't delete the record");
            }
        }
        
        public async Task DeleteRecordAsync(DateTime date)
        {
            var success = await recordRepository.DeleteRecordAsync(date);
            if (!success)
            {
                await Console.Error.WriteLineAsync("Couldn't delete the record");
            }
        }

        public async Task<Break> CreateBreakAsync(string breakType, DateTime inTimeBreak, DateTime outTimeBreak)
        {
            BreakType breakTypeEnum = (BreakType)Enum.Parse(typeof(BreakType), breakType, true); 
            var newBreak = await breakRepository.CreateBreakAsync(breakTypeEnum, inTimeBreak, outTimeBreak);
            return newBreak;
        }

        public async Task<Break> UpdateBreakAsync(string breakType, DateTime inTimeBreak, DateTime outTimeBreak, Break workBreak)
        {
            BreakType breakTypeEnum = (BreakType)Enum.Parse(typeof(BreakType), breakType, true);
            var updatedBreak = await breakRepository.UpdateBreakAsync(workBreak.Id, breakTypeEnum, inTimeBreak, outTimeBreak);
            return updatedBreak;
        }

        public async Task AddVacationAsync(DateTime vacationDay)
        {
            var vacationFromDb = await vacationRepository.GetVacationByDate(vacationDay);
            if (vacationFromDb != null)
            {
                await Console.Error.WriteLineAsync("Vacation on this day already exists");
                return;
            }

            var vacation = await vacationRepository.CreateVacationAsync(vacationDay);
            if (vacation != null)
            {
                await Console.Out.WriteLineAsync($"Added a new vacation: {vacation.Date}");
            }
        }

        public async Task DeleteVacationAsync(DateTime vacationDayToDelete)
        {
            var vacationFromDb = await vacationRepository.GetVacationByDate(vacationDayToDelete);
            var success = await vacationRepository.DeleteVacationAsync(vacationFromDb.Id);
            if (!success)
            {
                await Console.Error.WriteLineAsync("Couldn't delete the vacation");
            }
        }

        public async Task<List<Work>> GetAllRecordsByMonth(string month, string year)
        {
            List<Work> records = new List<Work>();

            for (int i = 1; i <= 30; i++)
            {
                var date = $"{i}/{month}/{year}";
                var record = await recordRepository.GetRecordByDateAsync(date);
                records.Add(record);
            }

            return records;
        }

        public async Task<List<Vacation>> GetAllVacationsByMonth(string month, string year)
        {
            List<Vacation> vacations = new List<Vacation>();

            for (int i = 1; i <= 30; i++)
            {
                var date = $"{i}/{month}/{year}";
                DateTime oDate = Convert.ToDateTime(date);
                var vacation = await vacationRepository.GetVacationByDateAsync(oDate);
                vacations.Add(vacation);
            }

            return vacations;
        }
    }
}