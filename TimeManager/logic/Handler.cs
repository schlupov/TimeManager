using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly Dictionary<int, string> Months = new()
        {
            {1, "Jan"}, {2, "Feb"}, {3, "Mar"}, {4, "Apr"}, {5, "May"}, {6, "Jun"}, {7, "Jul"}, {8, "Aug"}, {9, "Sep"},
            {10, "Oct"}, {11, "Nov"}, {12, "Dec"}
        };
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
                    Password = null, Email = null, Name = null, ChatLog = null
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
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
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

        private async Task<string> ReadTextAsync(string filePath)
        {
            await using var sourceStream =
                new FileStream(
                    filePath,
                    FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 4096, useAsync: true);

            var sb = new StringBuilder();

            byte[] buffer = new byte[0x1000];
            int numRead;
            while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string text = Encoding.UTF8.GetString(buffer, 0, numRead);
                sb.Append(text);
            }

            return sb.ToString();
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
                Name = newName ?? Configuration.Name,
                ChatLog = Configuration.ChatLog
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

                var work = await recordRepository.CreateRecordAsync(type, inTime.ToShortTimeString(),
                    outTime.ToShortTimeString(), dateTime.ToShortDateString(), comment);
                if (work == null)
                {
                    await Console.Error.WriteLineAsync("Couldn't create a new record");
                    return;
                }

                await userLogic.AddWorkToUserAsync(Configuration.Email, work);
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

            if (outTimeBreak.Hour < 8 || outTimeBreak.Hour >= 22)
            {
                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                return;
            }
            
            if (inTimeBreak.Hour < 8 || inTimeBreak.Hour >= 22)
            {
                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                return;
            }
            
            if (outTimeBreak < inTimeBreak)
            {
                await Console.Error.WriteLineAsync("You can't finish before you start ");
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

            if (outTimeBreak.Hour < 8 || outTimeBreak.Hour == 22 && outTimeBreak.Minute >= 1 || outTimeBreak.Hour >= 23)
            {
                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                return;
            }
            
            if (inTimeBreak.Hour < 8 || inTimeBreak.Hour == 22 && inTimeBreak.Minute >= 1 || inTimeBreak.Hour >= 23)
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

            var vacationFromDb =
                vacationRepository.GetVacationByDate(vacationDay.ToShortDateString(), Configuration.Email);
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
            if (!CheckMonthYear(month, year))
            {
                return;
            }

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

        private static bool CheckMonthYear(string month, string year)
        {
            int monthNumber;
            int yearNumber;
            try
            {
                monthNumber = Int32.Parse(month);
                yearNumber = Int32.Parse(year);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("Invalid month or year");
                return false;
            }

            if (yearNumber < 1975 || yearNumber > 2030)
            {
                Console.Error.WriteLine("Invalid year");
                return false;
            }

            if (monthNumber < 1 || monthNumber > 12)
            {
                Console.Error.WriteLine("Invalid month");
                return false;
            }

            return true;
        }

        private List<Vacation> GetAllVacationsByMonth(string month, string year)
        {
            List<Vacation> vacations = new List<Vacation>();

            var monthNumber = Int32.Parse(month);
            var yearNumber = Int32.Parse(year);
            int days = DateTime.DaysInMonth(yearNumber, monthNumber);
            for (int i = 1; i <= days; i++)
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

            var monthNumber = Int32.Parse(month);
            var yearNumber = Int32.Parse(year);
            int days = DateTime.DaysInMonth(yearNumber, monthNumber);
            for (int i = 1; i <= days; i++)
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

            var monthNumber = Int32.Parse(month);
            var yearNumber = Int32.Parse(year);
            int days = DateTime.DaysInMonth(yearNumber, monthNumber);
            for (int i = 1; i <= days; i++)
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

        public async Task HandleReadFromFileAsync(string month, string year)
        {
            if (!CheckMonthYear(month, year))
            {
                return;
            }
            string chatLog;
            if (Configuration.ChatLog == null)
            {
                await Console.Out.WriteLineAsync("Path to chat log file: ");
                chatLog = await Console.In.ReadLineAsync();
            }
            else
            {
                await Console.Out.WriteLineAsync(
                    $"You have set the path to the chat log file in config.json to {Configuration.ChatLog}, do you wish to use it? [y/n]");
                var response = await Console.In.ReadLineAsync();
                if (response != null && response.ToLower() == "y")
                {
                    chatLog = Configuration.ChatLog;
                }
                else
                {
                    await Console.Out.WriteLineAsync("The path to the chat log file: ");
                    chatLog = await Console.In.ReadLineAsync();
                }
            }

            if (chatLog == null)
            {
                await Console.Error.WriteLineAsync("The path to the chat log file is not specified!");
                return;
            }

            try
            {
                var chatLogAbsolute = Path.GetFullPath((new Uri(chatLog!)).LocalPath);
                if (File.Exists(chatLogAbsolute))
                {
                    string text = await ReadTextAsync(chatLogAbsolute);
                    int monthNumber;
                    try
                    {
                        monthNumber = Int32.Parse(month);
                    }
                    catch (FormatException)
                    {
                        await Console.Error.WriteLineAsync("Invalid month");
                        return;
                    }
                    var patternStart =
                        new Regex(
                            @"(BEGIN LOGGING AT (?<day>Mon|Tue|Wed|Thu|Fri|Sat|Sun) "
                            + @"(?<month>" + Months[monthNumber] + @") "
                            + @"(?<date> \d|\d\d) (?<time>(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]) "
                            + @"(?<year>" + year + @"))");
                    var patternEnd =
                        new Regex(
                            @"(ENDING LOGGING AT (?<day>Mon|Tue|Wed|Thu|Fri|Sat|Sun) "
                            + @"(?<month>" + Months[monthNumber] + @") "
                            + @"(?<date> \d|\d\d) (?<time>(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]) "
                            + @"(?<year>" + year + @"))");
                    var matchesStart = patternStart.Matches(text);
                    var matchesEnd = patternEnd.Matches(text);
                    if (matchesStart.Count == 0 || matchesEnd.Count == 0)
                    {
                        await Console.Error.WriteLineAsync("Didn't find any suitable record");
                        return;
                    }
                    int j = 0;
                    int i = 0;
                    string previousDate = null;
                    while (i < matchesStart.Count) 
                    {
                        var outTime = DateTime.Parse(matchesEnd[j].Groups["time"].Value, new CultureInfo("cs-CZ"));
                        StringBuilder sb = new StringBuilder();
                        sb.Append(matchesStart[i].Groups["day"].Value).Append(' ')
                            .Append(matchesStart[i].Groups["month"].Value).Append(' ')
                            .Append(matchesStart[i].Groups["date"].Value).Append(' ')
                            .Append(matchesStart[i].Groups["time"].Value).Append(' ')
                            .Append(matchesStart[i].Groups["year"].Value);
                        var inTime = DateTime.Parse(matchesStart[i].Groups["time"].Value, new CultureInfo("cs-CZ"));
                        StringBuilder sbDate = new StringBuilder();
                        sbDate.Append(matchesStart[i].Groups["date"].Value).Append('/')
                            .Append(month).Append('/')
                            .Append(matchesStart[i].Groups["year"].Value);
                        if (previousDate == sbDate.ToString())
                        {
                            i++;
                            continue;
                        }

                        var date = DateTime.Parse(sbDate.ToString(), new CultureInfo("cs-CZ"));
                        previousDate = sbDate.ToString();
                        await Console.Out.WriteLineAsync($"Found work: {sb}, do you wish to save this work? [y/n]");
                        var response = await Console.In.ReadLineAsync();
                        if (response != null && response.ToLower() == "y")
                        {
                            if (inTime.Hour < 8 || inTime.Hour >= 22)
                            {
                                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                                j++;
                                i++;
                                continue;
                            }

                            if (outTime.Hour < 8 || outTime.Hour >= 22)
                            {
                                await Console.Error.WriteLineAsync("Working hours are from 8 to 22");
                                j++;
                                i++;
                                continue;
                            }

                            await Console.Out.WriteAsync("Type [bugzilla|issue|documentation|release]: ");
                            var typeString = await Console.In.ReadLineAsync();
                            WorkType type;
                            try
                            {
                                type = (WorkType) Enum.Parse(typeof(WorkType), typeString!, true);
                            }
                            catch (ArgumentException exception)
                            {
                                await Console.Error.WriteLineAsync(exception.Message);
                                j++;
                                i++;
                                continue;
                            }

                            var work = await recordRepository.CreateRecordAsync(type, inTime.ToShortTimeString(),
                                outTime.ToShortTimeString(), date.ToShortDateString(), null);
                            if (work == null)
                            {
                                await Console.Error.WriteLineAsync("Couldn't create a new record");
                                j++;
                                i++;
                                continue;
                            }

                            await userLogic.AddWorkToUserAsync(Configuration.Email, work);
                        }

                        j++;
                        i++;
                    }
                }
                else
                {
                    await Console.Error.WriteLineAsync($"File not found: {chatLogAbsolute}");
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }
    }
}