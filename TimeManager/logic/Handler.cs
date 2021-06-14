using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ConsoleTables;
using DAL;
using DAL.models;
using DAL.repository;
using Newtonsoft.Json;
using TimeManager.configuration;

namespace TimeManager.logic
{
    public class Handler
    {
        private readonly UserLogic userLogic = new();
        private readonly RecordRepository recordRepository = new();
        private readonly BreakRepository breakRepository = new();
        private readonly VacationRepository vacationRepository = new();
        public Configuration Configuration;

        public async Task ReadConfigAsync()
        {
            string configFile =
                Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../configuration/"),
                    "config.json");
            if (File.Exists(configFile))
            {
                Configuration =
                    JsonConvert.DeserializeObject<Configuration>(await File.ReadAllTextAsync(configFile));
            }
            else
            {
                Configuration = new Configuration
                {
                    Password = null, Email = null, Name = null
                };
            }
        }

        public async Task HandleCreateAsync()
        {
            string name = null;
            string email = null;
            string password = null;
            bool success = false;

            while (!success)
            {
                await Console.Out.WriteAsync("Name: ");
                name = await Console.In.ReadLineAsync();
                await Console.Out.WriteAsync("Email: ");
                email = await Console.In.ReadLineAsync();
                await Console.Out.WriteAsync("Password: ");
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

                if (!IsValidEmail(email))
                {
                    await Console.Error.WriteLineAsync("Invalid email");
                    email = null;
                }

                if (name != null && email != null && password != null)
                {
                    success = true;
                }
            }

            Configuration.Email = email;
            Configuration.Password = password;
            Configuration.Name = name;
            await userLogic.CreateUserAsync(name, email, password);
        }
        
        private bool IsValidEmail(string email)
        {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch {
                return false;
            }
        }
        
        public async Task<string> HandleLogicAsync(string email, string password)
        {
            bool successPassword = await userLogic.CheckPasswordAsync(email, password);
            if (!successPassword)
            {
                return null;
            }

            var user = await userLogic.ReadUserSettingsAsync(email);
            if (user == null || Configuration.Name == user.Name)
            {
                return user?.Name;
            }

            await Console.Error.WriteLineAsync($"Configuring user name to {user.Name}");
            Configuration.Name = user.Name;
            return user.Name;
        }

        public async Task HandleUpdateAsync(string update)
        {
            switch (update)
            {
                case "password":
                    var newPassword = await userLogic.UpdatePasswordAsync(Configuration.Email);
                    Configuration.Password = newPassword;
                    await Console.Out.WriteLineAsync("Do you wish to update the config.json file? [y/n]");
                    var response = await Console.In.ReadLineAsync();
                    if (response != null && response.ToLower() == "y")
                    {
                        await UpdateJsonFileAsync(newPassword, null, null);
                    }

                    break;
                case "name":
                    var newName = await userLogic.UpdateNameAsync(Configuration.Email);
                    Configuration.Name = newName;
                    await Console.Out.WriteLineAsync("Do you wish to update the config.json file? [y/n]");
                    var responseName = await Console.In.ReadLineAsync();
                    if (responseName != null && responseName.ToLower() == "y")
                    {
                        await UpdateJsonFileAsync(null, null, newName);
                    }

                    break;
                default:
                    await Console.Error.WriteLineAsync("Bad choice, choose email, password or name");
                    break;
            }
        }

        private async Task UpdateJsonFileAsync(string newPassword, string newEmail, string newName)
        {
            string configFile =
                Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../configuration/"),
                    "config.json");
            await File.WriteAllTextAsync(configFile, String.Empty);
            await using var stream = File.OpenWrite(configFile);
            await WriteToFileAsync(string.Empty, stream);

            Configuration newConfiguration = new Configuration
            {
                Password = newPassword ?? Configuration.Password,
                Email = newEmail ?? Configuration.Email,
                Name = newName ?? Configuration.Name
            };
            string output = JsonConvert.SerializeObject(newConfiguration, Formatting.Indented);
            await WriteToFileAsync(output, stream);
        }

        private static async Task WriteToFileAsync(string contentToWrite, Stream stream)
        {
            var data = new UTF8Encoding(true).GetBytes(contentToWrite);
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task HandleDeleteAsync()
        {
            bool success = await userLogic.DeleteUserAsync(Configuration.Email);
            if (success)
            {
                Environment.Exit(0);
            }
        }

        public async Task HandleCreateRecordAsync()
        {
            await Console.Out.WriteAsync("Type [bugzilla|issue|documentation|release]: ");
            var typeString = await Console.In.ReadLineAsync();
            WorkType type;
            try
            {
                type = (WorkType) Enum.Parse(typeof(WorkType), typeString, true);
            }
            catch (ArgumentException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }

            await Console.Out.WriteAsync("When the work started (e.g. 12:45): ");
            var inTimeString = await Console.In.ReadLineAsync();
            try
            {
                var inTime = DateTime.Parse(inTimeString, new CultureInfo("cs-CZ"));
                if (inTime.Hour < 8 || inTime.Hour >= 22)
                {
                    await Console.Error.WriteLineAsync("Working hours are from 8:00 to 21:59");
                    return;
                }

                await Console.Out.WriteAsync("When the work ended (e.g. 14:00): ");
                var outTimeString = await Console.In.ReadLineAsync();
                var outTime = DateTime.Parse(outTimeString, new CultureInfo("cs-CZ"));
                if (outTime.Hour < 8 || outTime.Hour >= 22)
                {
                    await Console.Error.WriteLineAsync("Working hours are from 8:00 to 21:59");
                    return;
                }

                if (outTime < inTime)
                {
                    await Console.Error.WriteLineAsync("You can't finish before you start ");
                    return;
                }

                await Console.Out.WriteAsync("Date (e.g. 31/5/2012): ");
                var date = await Console.In.ReadLineAsync();
                var dateTime = DateTime.Parse(date, new CultureInfo("cs-CZ"));

                await Console.Out.WriteAsync("Comment (not required): ");
                var comment = await Console.In.ReadLineAsync();
                
                var user = await userLogic.ReadUserSettingsAsync(Configuration.Email);
                var work = await recordRepository.CreateRecordAsync(type, inTime.ToShortTimeString(),
                    outTime.ToShortTimeString(), dateTime.ToShortDateString(), comment);
                if (work == null)
                {
                    await Console.Error.WriteLineAsync("Couldn't create a new record");
                    return;
                }

                await userLogic.AddWorkToUserAsync(user, work);
            }
            catch (FormatException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
            }
        }

        public async Task HandleAddBreakAsync()
        {
            string breakType;
            string date;
            DateTime inTimeBreak;
            DateTime outTimeBreak;
            
            try
            {
                (breakType, date, inTimeBreak, outTimeBreak) =
                    await AskInfoAboutBreakAsync();
            }
            catch (FormatException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }

            var work = recordRepository.ReadRecordByDate(date, Configuration.Email);
            if (work.Count == 0)
            {
                await Console.Error.WriteLineAsync("You didn't work that day");
                return;
            }

            if (inTimeBreak.Hour < 8 || outTimeBreak.Hour == 22 && outTimeBreak.Minute >= 1 || outTimeBreak.Hour >= 23)
            {
                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                return;
            }

            BreakType breakTypeEnum;
            try
            {
                breakTypeEnum = (BreakType) Enum.Parse(typeof(BreakType), breakType, true);
            }
            catch (ArgumentException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }

            var newBreak = await breakRepository.CreateBreakAsync(breakTypeEnum, inTimeBreak.ToShortTimeString(),
                outTimeBreak.ToShortTimeString(), date);
            if (newBreak != null)
            {
                await Console.Out.WriteLineAsync($"Added break of type: {newBreak.Type}");
                await userLogic.AddBreakToUserAsync(Configuration.Email, newBreak);
            }
            else
            {
                await Console.Error.WriteLineAsync("It wasn't possible to add a new break");
            }
        }

        public async Task HandleUpdateBreakAsync()
        {
            await Console.Out.WriteAsync("Choose break date (e.g. 31/5/2012): ");
            var dateString = await Console.In.ReadLineAsync();
            DateTime desiredDate;
            try
            {
                desiredDate = DateTime.Parse(dateString, new CultureInfo("cs-CZ"));
            }
            catch (FormatException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }

            var bBreak = breakRepository.ReadBreakByDate(desiredDate.ToShortDateString(), Configuration.Email);
            string breakType;
            string date;
            DateTime inTimeBreak;
            DateTime outTimeBreak;
            
            try
            {
                (breakType, date, inTimeBreak, outTimeBreak) =
                    await AskInfoAboutBreakAsync();
            }
            catch (FormatException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }
            
            var work = recordRepository.ReadRecordByDate(date, Configuration.Email);
            if (work.Count == 0)
            {
                await Console.Error.WriteLineAsync("You didn't work that day");
                return;
            }

            if (inTimeBreak.Hour < 8 || outTimeBreak.Hour == 22 && outTimeBreak.Minute >= 1 || outTimeBreak.Hour >= 23)
            {
                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                return;
            }

            BreakType breakTypeEnum;
            try
            {
                breakTypeEnum = (BreakType) Enum.Parse(typeof(BreakType), breakType, true);
            }
            catch (ArgumentException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }

            var newBreak = await breakRepository.UpdateBreakAsync(bBreak.Id, breakTypeEnum,
                inTimeBreak.ToShortTimeString(), outTimeBreak.ToShortTimeString());
            if (newBreak != null)
            {
                await Console.Out.WriteLineAsync($"Updated break of type: {newBreak.Type}");
            }
            else
            {
                await Console.Error.WriteLineAsync("It wasn't possible to update the break");
            }
        }

        private async Task<(string, string, DateTime, DateTime)> AskInfoAboutBreakAsync()
        {
            await Console.Out.WriteAsync("Type [lunch|snack|doctor]: ");
            var breakType = await Console.In.ReadLineAsync();

            await Console.Out.WriteAsync("Date (e.g. 31/5/2012): ");
            var date = await Console.In.ReadLineAsync();
            var dateTime = DateTime.Parse(date, new CultureInfo("cs-CZ"));

            await Console.Out.WriteAsync("When the break started (e.g. 12:45): ");
            var inTimeBreak = await Console.In.ReadLineAsync();
            var inTime = DateTime.Parse(inTimeBreak, new CultureInfo("cs-CZ"));

            await Console.Out.WriteAsync("When the break ended (e.g. 14:00): ");
            var outTimeBreak = await Console.In.ReadLineAsync();
            var outTime = DateTime.Parse(outTimeBreak, new CultureInfo("cs-CZ"));
            return (breakType, dateTime.ToShortDateString(), inTime, outTime);
        }

        public void HandleReadRecord(string day)
        {
            DateTime dayTime;
            try
            {
                dayTime = DateTime.Parse(day, new CultureInfo("cs-CZ"));
            }
            catch (FormatException exception)
            {
                Console.Error.WriteLine(exception.Message);
                return;
            }

            var work = recordRepository.ReadRecordByDate(dayTime.ToShortDateString(), Configuration.Email);
            if (work.Count != 0)
            {
                var table = new ConsoleTable("id", "type", "in", "out", "date", "comment");
                foreach (var record in work)
                {
                    table.AddRow(record.Id, record.Type, record.In, record.Out, record.Date, record.Comment);
                }

                table.Write();
            }
        }

        public async Task HandleUpdateRecordAsync(string id)
        {
            int result;
            try
            {
                result = Int32.Parse(id);
            }
            catch (FormatException)
            {
                await Console.Error.WriteAsync("Unknown command");
                return;
            }

            var success = recordRepository.CheckUserRecord(Configuration.Email, result);
            if (!success)
            {
                await Console.Error.WriteLineAsync($"You don't have a work with id {result}");
                return;
            }
            var work = await recordRepository.GetRecordAsync(result);
            if (work != null)
            {
                await Console.Out.WriteLineAsync("Choose what you want to update [1-5]:");
                await Console.Out.WriteLineAsync("\t1. Type");
                await Console.Out.WriteLineAsync("\t2. In");
                await Console.Out.WriteLineAsync("\t3. Out");
                await Console.Out.WriteLineAsync("\t4. Date");
                await Console.Out.WriteLineAsync("\t5. Comment");
                var number = await Console.In.ReadLineAsync();
                switch (number)
                {
                    case "1":
                        await Console.Out.WriteAsync("New type [bugzilla|issue|documentation|release]: ");
                        var newType = await Console.In.ReadLineAsync();
                        await UpdateTypeAsync(newType, result);
                        break;
                    case "2":
                        await Console.Out.WriteAsync("New in time: ");
                        var inTimeString = await Console.In.ReadLineAsync();
                        if (!ProcessInTime(inTimeString, work, out var inTime))
                        {
                            break;
                        }
                        await recordRepository.UpdateInTimeAsync(inTime.ToShortTimeString(), result);
                        break;
                    case "3":
                        await Console.Out.WriteAsync("New out time: ");
                        var outTimeString = await Console.In.ReadLineAsync();
                        if (!ProcessOutTime(outTimeString, work, out var outTime))
                        {
                            break;
                        }
                        await recordRepository.UpdateOutTimeAsync(outTime.ToShortTimeString(), result);
                        break;
                    case "4":
                        await Console.Out.WriteAsync("New date: ");
                        var date = await Console.In.ReadLineAsync();
                        if (!ProcessDate(date, out var dateTime))
                        {
                            break;
                        }
                        await recordRepository.UpdateDateAsync(dateTime.ToShortDateString(), result);
                        break;
                    case "5":
                        await Console.Out.WriteAsync("New comment: ");
                        var comment = await Console.In.ReadLineAsync();
                        if (comment is {Length: > 10})
                        {
                            await Console.Error.WriteLineAsync("Comment is too long");
                            return;
                        }
                        await UpdateCommentAsync(comment, result);
                        break;
                    default:
                        await Console.Error.WriteAsync("Unknown command");
                        break;
                }
            }
        }

        private static bool ProcessDate(string date, out DateTime dateTime)
        {
            try
            {
                dateTime = DateTime.Parse(date, new CultureInfo("cs-CZ"));
            }
            catch (FormatException exception)
            {
                Console.Error.WriteLine(exception.Message);
                dateTime = new DateTime();
                return false;
            }

            return true;
        }

        private static bool ProcessOutTime(string outTimeString, Work work, out DateTime outTime)
        {
            DateTime inFromDb;
            try
            {
                outTime = DateTime.Parse(outTimeString, new CultureInfo("cs-CZ"));
                inFromDb = DateTime.Parse(work.In, new CultureInfo("cs-CZ"));
            }
            catch (FormatException exception)
            {
                Console.Error.WriteLine(exception.Message);
                outTime = new DateTime();
                return false;
            }

            if (outTime.Hour < 8 || outTime.Hour >= 22)
            {
                Console.Error.WriteLine("Working hours are from 8:00 to 21:59");
                return false;
            }

            if (inFromDb > outTime)
            {
                Console.Error.WriteLine("You can't finish before you start ");
                return false;
            }

            return true;
        }

        private static bool ProcessInTime(string inTimeString, Work work, out DateTime inTime)
        {
            DateTime outFromDb;
            try
            {
                inTime = DateTime.Parse(inTimeString, new CultureInfo("cs-CZ"));
                outFromDb = DateTime.Parse(work.Out, new CultureInfo("cs-CZ"));
            }
            catch (FormatException exception)
            {
                Console.Error.WriteLine(exception.Message);
                inTime = new DateTime();
                return false;
            }

            if (inTime.Hour < 8 || inTime.Hour >= 22)
            {
                Console.Error.WriteLine("Working hours are from 8:00 to 21:59");
                return false;
            }

            if (outFromDb < inTime)
            {
                Console.Error.WriteLine("You can't finish before you start ");
                return false;
            }

            return true;
        }

        private async Task UpdateCommentAsync(string comment, int id)
        {
            if (comment is {Length: > 10})
            {
                await Console.Error.WriteLineAsync("Comment is too long");
                return;
            }

            await recordRepository.UpdateCommentAsync(comment, id);
        }

        private async Task UpdateTypeAsync(string newType, int id)
        {
            WorkType type;
            try
            {
                type = (WorkType) Enum.Parse(typeof(WorkType), newType, true);
            }
            catch (ArgumentException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }

            await recordRepository.UpdateTypeAsync(type, id);
        }

        public async Task HandleDeleteRecordAsync(string column)
        {
            bool success;
            try
            {
                DateTime oDate = DateTime.Parse(column, new CultureInfo("cs-CZ"));
                success = await recordRepository.DeleteRecordAsync(oDate, Configuration.Email);
            }
            catch (FormatException)
            {
                bool isConverted = Int32.TryParse(column, out var id);
                if (isConverted)
                {
                    success = await recordRepository.DeleteRecordAsync(id);
                }
                else
                {
                    success = false;
                }
            }
            if (!success)
            {
                await Console.Error.WriteLineAsync("Couldn't delete the record");
            }
        }

        public async Task HandleAddVacationAsync(string vacationDayString)
        {
            DateTime vacationDay;
            try
            {
                vacationDay = DateTime.Parse(vacationDayString, new CultureInfo("cs-CZ"));
            }
            catch (FormatException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }

            var vacationFromDb = vacationRepository.GetVacationByDate(vacationDay.ToShortDateString(), Configuration.Email);
            if (vacationFromDb != null)
            {
                await Console.Error.WriteLineAsync("Vacation on this day already exists");
                return;
            }

            var work = recordRepository.ReadRecordByDate(vacationDay.ToShortDateString(), Configuration.Email);
            if (work.Count != 0)
            {
                await Console.Error.WriteLineAsync("Can't add a vacation, you have a work record that day");
                return;
            }

            var vacation = await vacationRepository.CreateVacationAsync(vacationDay.ToShortDateString());
            if (vacation != null)
            {
                await Console.Out.WriteLineAsync($"Added a new vacation: {vacation.Date}");
                await userLogic.AddVacationToUserAsync(Configuration.Email, vacation);
            }
        }

        public async Task HandleDeleteVacationAsync(string vacationDayToDelete)
        {
            var vacationFromDb = vacationRepository.GetVacationByDate(vacationDayToDelete, Configuration.Email);
            if (vacationFromDb == null)
            {
                await Console.Error.WriteLineAsync("Couldn't delete the vacation");
                return;
            }
            var success = await vacationRepository.DeleteVacationAsync(vacationFromDb.Id);
            if (!success)
            {
                await Console.Error.WriteLineAsync("Couldn't delete the vacation");
            }
        }

        public void HandleShowHistory(string month, string year)
        {
            var records = GetAllRecordsByMonth(month, year);
            if (records.Count != 0)
            {
                var table = new ConsoleTable("id", "work", "in", "out", "date", "comment");
                foreach (var record in records)
                {
                    table.AddRow(record.Id, record.Type, record.In, record.Out, record.Date, record.Comment);
                }

                table.Write();
            }

            var bBreaks = GetAllBreaksByMonth(month, year);
            if (bBreaks.Count != 0)
            {
                var table = new ConsoleTable("id", "break", "in", "out", "date");
                foreach (var bBreak in bBreaks)
                {
                    table.AddRow(bBreak.Id, bBreak.Type, bBreak.In, bBreak.Out, bBreak.Date);
                }

                table.Write();
            }

            var vacations = GetAllVacationsByMonth(month, year);
            if (vacations.Count != 0)
            {
                var table = new ConsoleTable("id", "vacation");
                foreach (var vacation in vacations)
                {
                    table.AddRow(vacation.Id, vacation.Date);
                }

                table.Write();
            }
        }

        private List<Vacation> GetAllVacationsByMonth(string month, string year)
        {
            List<Vacation> vacations = new List<Vacation>();

            // TODO: brat vsechny dny podle mesice
            for (int i = 1; i <= 30; i++)
            {
                var date = $"{month}/{i}/{year}";
                var vacation = vacationRepository.GetVacationByDate(date, Configuration.Email);
                if (vacation != null)
                {
                    vacations.Add(vacation);
                }
            }

            return vacations;
        }

        private List<Work> GetAllRecordsByMonth(string month, string year)
        {
            List<Work> records = new List<Work>();

            for (int i = 1; i <= 30; i++)
            {
                var date = $"{month}/{i}/{year}";
                var record = recordRepository.ReadRecordByDate(date, Configuration.Email);
                if (record != null)
                {
                    records.AddRange(record);
                }
            }

            return records;
        }

        private List<Break> GetAllBreaksByMonth(string month, string year)
        {
            List<Break> breaks = new List<Break>();

            for (int i = 1; i <= 30; i++)
            {
                var date = $"{month}/{i}/{year}";
                var bBreak = breakRepository.ReadBreakByDate(date, Configuration.Email);
                if (bBreak != null)
                {
                    breaks.Add(bBreak);
                }
            }

            return breaks;
        }

        public async Task<User> HandleReadUsersByEmailAsync(string email)
        {
            var user = await userLogic.ReadUserSettingsAsync(email);
            return user;
        }

        public async Task HandleDeleteBreakAsync()
        {
            await Console.Out.WriteAsync("Choose break date (e.g. 31/5/2012): ");
            var dateString = await Console.In.ReadLineAsync();

            DateTime desiredDate;
            try
            {
                desiredDate = DateTime.Parse(dateString, new CultureInfo("cs-CZ"));
            }
            catch (FormatException exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
                return;
            }

            await breakRepository.DeleteBreakAsync(desiredDate.ToShortDateString(), Configuration.Email);
        }
    }
}