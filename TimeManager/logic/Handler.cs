using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTables;
using DAL;
using DAL.models;

namespace TimeManager.logic
{
    public class Handler
    {
        private readonly UserLogic userLogic = new();
        private readonly RecordLogic recordLogic = new();

        public async Task HandleCreateAsync()
        {
            string name = null;
            string email = null;
            string password = null;
            bool success = false;

            while (!success)
            {
                await Console.Out.WriteLineAsync("Name:");
                name = await Console.In.ReadLineAsync();
                await Console.Out.WriteLineAsync("Email:");
                email = await Console.In.ReadLineAsync();
                await Console.Out.WriteLineAsync("Password:");
                password = await Console.In.ReadLineAsync();

                if (name is {Length: <= 3})
                {
                    await Console.Error.WriteLineAsync("Name is too short");
                    name = null;
                }

                if (password is {Length: <= 3})
                {
                    await Console.Error.WriteLineAsync("Password is too short");
                    password = null;
                }

                if (name != null && email != null && password != null)
                {
                    success = true;
                }
            }

            Program.Configuration.Email = email;
            Program.Configuration.Password = password;
            Program.Configuration.Name = name;
            await userLogic.CreateUserAsync(name, email, password);
        }

        public async Task<string> HandleLogicAsync(string email, string password)
        {
            bool successPassword = await userLogic.CheckPasswordAsync(email, password);
            if (!successPassword)
            {
                return null;
            }

            var user = await userLogic.ReadUserSettingsAsync(email);
            if (user == null || Program.Configuration.Name == user.Name)
            {
                return user?.Name;
            }

            await Console.Error.WriteLineAsync($"Configuring user name to {user.Name}");
            Program.Configuration.Name = user.Name;
            return user.Name;
        }

        public async Task HandleUpdateAsync(string update)
        {
            switch (update)
            {
                case "email":
                    await userLogic.UpdateEmailAsync(Program.Configuration.Email);
                    break;
                case "password":
                    await userLogic.UpdatePasswordAsync(Program.Configuration.Email);
                    break;
                case "name":
                    await userLogic.UpdateNameAsync(Program.Configuration.Email);
                    break;
                default:
                    await Console.Error.WriteLineAsync("Bad choice, choose email, password or name");
                    break;
            }
        }

        public async Task HandleDeleteAsync()
        {
            bool success = await userLogic.DeleteUserAsync(Program.Configuration.Email);
            if (success)
            {
                Environment.Exit(0);
            }
        }

        public async Task HandleCreateRecordAsync()
        {
            await Console.Out.WriteLineAsync("Type [bugzilla|issue|documentation|release]: ");
            var type = await Console.In.ReadLineAsync();
            await Console.Out.WriteLineAsync("When the work started (e.g. 12:45): ");
            var inTimeString = await Console.In.ReadLineAsync();
            var inTime = DateTime.Parse(inTimeString, new CultureInfo("cs-CZ"));
            if (inTime.Hour < 8)
            {
                await Console.Error.WriteLineAsync("Working hours start at 8");
                return;
            }

            await Console.Out.WriteLineAsync("When the work ended (e.g. 14:00): ");
            var outTimeString = await Console.In.ReadLineAsync();
            var outTime = DateTime.Parse(outTimeString, new CultureInfo("cs-CZ"));
            if (outTime.Hour == 22 && outTime.Minute > 1 || outTime.Hour > 22)
            {
                await Console.Error.WriteLineAsync("Working hours end at 22");
                return;
            }

            await Console.Out.WriteLineAsync("Date (e.g. 31/5/2012): ");
            var date = await Console.In.ReadLineAsync();
            var dateTime = DateTime.Parse(date, new CultureInfo("cs-CZ"));
            var recordsFromDb = await recordLogic.ReadRecordByDateAsync(dateTime);
            if (recordsFromDb.Count != 0)
            {
                await Console.Error.WriteLineAsync("A work record already exists on this day");
                return;
            }

            await Console.Out.WriteLineAsync("Comment (not required): ");
            var comment = await Console.In.ReadLineAsync();
            var success =
                await recordLogic.CreateRecordAsync(type, inTime, outTime, dateTime.ToShortDateString(), comment);
            if (!success)
            {
                await Console.Error.WriteLineAsync("Couldn't create a new record");
                return;
            }

            await Console.Out.WriteLineAsync("Do you wish to add a break? [y/n]");
            var response = await Console.In.ReadLineAsync();
            if (response != null && response.ToLower() == "y")
            {
                await HandleAddBreakAsync(inTime, outTime);
            }
        }

        private async Task HandleAddBreakAsync(DateTime workStarted, DateTime workEnded)
        {
            (string breakType, DateTime inTimeBreak, DateTime outTimeBreak) = await AskInfoAboutBreakAsync();
            if (outTimeBreak > workEnded || inTimeBreak < workStarted)
            {
                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                return;
            }

            var newBreak = await recordLogic.CreateBreakAsync(breakType, inTimeBreak, outTimeBreak);
            if (newBreak != null)
            {
                await Console.Out.WriteLineAsync($"Added break of type: {newBreak.Type}");
            }
            else
            {
                await Console.Error.WriteLineAsync("It wasn't possible to add a new break");
            }
        }
        
        private async Task HandleUpdateBreakAsync(Work work)
        {
            DateTime workStarted = work.In;
            DateTime workEnded = work.Out;
            (string breakType, DateTime inTimeBreak, DateTime outTimeBreak) = await AskInfoAboutBreakAsync();
            if (outTimeBreak > workEnded || inTimeBreak < workStarted)
            {
                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                return;
            }

            var newBreak = await recordLogic.UpdateBreakAsync(breakType, inTimeBreak, outTimeBreak, work.Break);
            if (newBreak != null)
            {
                await Console.Out.WriteLineAsync($"Updated break of type: {newBreak.Type}");
            }
            else
            {
                await Console.Error.WriteLineAsync("It wasn't possible to update the break");
            }
        }

        private async Task<(string, DateTime, DateTime)> AskInfoAboutBreakAsync()
        {
            await Console.Out.WriteLineAsync("Type [lunch|snack|doctor]: ");
            var breakType = await Console.In.ReadLineAsync();

            await Console.Out.WriteLineAsync("When the break started (e.g. 12:45): ");
            var inTimeBreakString = await Console.In.ReadLineAsync();
            var inTimeBreak = DateTime.Parse(inTimeBreakString, new CultureInfo("cs-CZ"));

            await Console.Out.WriteLineAsync("When the break ended (e.g. 14:00): ");
            var outTimeBreakString = await Console.In.ReadLineAsync();
            var outTimeBreak = DateTime.Parse(outTimeBreakString, new CultureInfo("cs-CZ"));
            return (breakType, inTimeBreak, outTimeBreak);
        }

        public async Task HandleReadRecordAsync(string day)
        {
            var dayTime = DateTime.Parse(day, new CultureInfo("cs-CZ"));
            var work = await recordLogic.ReadRecordByDateAsync(dayTime);
            if (work.Count != 0)
            {
                var table = new ConsoleTable("type", "in", "out", "date", "comment");
                foreach (var record in work)
                {
                    table.AddRow(record.Type, record.In, record.Out, record.Date, record.Comment);
                }
                
                table.Write();
            }
        }

        public async Task HandleUpdateRecordAsync(string id)
        {
            var work = await recordLogic.GetRecordAsync(id);
            if (work != null)
            {
                await Console.Out.WriteLineAsync("Choose what you want to update [1-6]:");
                await Console.Out.WriteLineAsync("\t1. Type");
                await Console.Out.WriteLineAsync("\t2. In");
                await Console.Out.WriteLineAsync("\t3. Out");
                await Console.Out.WriteLineAsync("\t4. Date");
                await Console.Out.WriteLineAsync("\t5. Comment");
                await Console.Out.WriteLineAsync("\t6. Break");
                var number = await Console.In.ReadLineAsync();
                switch (number)
                {
                    case "1":
                        await Console.Out.WriteLineAsync("New type [bugzilla|issue|documentation|release]: ");
                        var newType = await Console.In.ReadLineAsync();
                        await recordLogic.UpdateTypeAsync(newType, id);
                        break;
                    case "2":
                        await Console.Out.WriteLineAsync("New in time: ");
                        var inTime = await Console.In.ReadLineAsync();
                        await recordLogic.UpdateInTimeAsync(inTime, id);
                        break;
                    case "3":
                        await Console.Out.WriteLineAsync("New out time: ");
                        var outTime = await Console.In.ReadLineAsync();
                        await recordLogic.UpdateOutTimeAsync(outTime, id);
                        break;
                    case "4":
                        await Console.Out.WriteLineAsync("New date: ");
                        var date = await Console.In.ReadLineAsync();
                        await recordLogic.UpdateDateAsync(date, id);
                        break;
                    case "5":
                        await Console.Out.WriteLineAsync("New comment: ");
                        var comment = await Console.In.ReadLineAsync();
                        await recordLogic.UpdateCommentAsync(comment, id);
                        break;
                    case "6":
                        await HandleUpdateBreakAsync(work);
                        break;
                }
            }
        }

        public async Task HandleDeleteRecordAsync(string id)
        {
            await recordLogic.DeleteRecordAsync(id);
        }
        
        public async Task HandleDeleteRecordAsync(DateTime date)
        {
            await recordLogic.DeleteRecordAsync(date);
        }

        public async Task HandleAddVacationAsync(DateTime vacationDay)
        {
            await recordLogic.AddVacationAsync(vacationDay);
        }

        public async Task HandleDeleteVacationAsync(DateTime vacationDayToDelete)
        {
            await recordLogic.DeleteVacationAsync(vacationDayToDelete);
        }

        public async Task HandleShowHistoryAsync(string month, string year)
        {
            var records = await recordLogic.GetAllRecordsByMonth(month, year);
            if (records.Count != 0)
            {
                var table = new ConsoleTable("type", "in", "out", "date", "comment", "break");
                foreach (var record in records)
                {
                    table.AddRow(record.Type, record.In, record.Out, record.Date, record.Comment, record.Break.Type);
                }
                
                table.Write();
            }
            
            var vacations = await recordLogic.GetAllVacationsByMonth(month, year);
            if (vacations.Count != 0)
            {
                var table = new ConsoleTable("date");
                foreach (var vacation in vacations)
                {
                    table.AddRow(vacation.Date);
                }
                
                table.Write();
            }
        }
    }
}