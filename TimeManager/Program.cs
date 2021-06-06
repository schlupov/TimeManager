using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimeManager.configuration;
using TimeManager.logic;


namespace TimeManager
{
    class Program
    {
        public static Configuration Configuration { get; set; }
        private static readonly Handler handler = new();

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            await Console.Out.WriteLineAsync("Do you wish to login or register? [login|register]");
            var response = await Console.In.ReadLineAsync();
            switch (response)
            {
                case "register":
                    await Console.Out.WriteLineAsync("Creating a new user");
                    await handler.HandleCreateAsync();
                    break;
                case "login":
                    await ReadConfigAsync();
                    await CheckEmailAsync();
                    await CheckPasswordAsync();
                    var name = await handler.HandleLogicAsync(Configuration.Email, Configuration.Password);
                    if (name == null)
                    {
                        await Console.Error.WriteLineAsync("Bad credentials");
                        Environment.Exit(1);
                    }
                    await Console.Out.WriteLineAsync($"Welcome {name}!");
                    break;
            }
            
            await ShowActionsAsync();
            await RunAsync();
        }

        private static async Task ShowActionsAsync()
        {
            await Console.Out.WriteLineAsync("Possible actions:");
            await Console.Out.WriteLineAsync("\tRead user settings [1]");
            await Console.Out.WriteLineAsync("\tUpdate user settings [2]");
            await Console.Out.WriteLineAsync("\tDelete user account [3]");
            await Console.Out.WriteLineAsync("\tCreate a new time record [4]");
            await Console.Out.WriteLineAsync("\tRead records by day [5]");
            await Console.Out.WriteLineAsync("\tUpdate a record by id [6]");
            await Console.Out.WriteLineAsync("\tDelete a record by id [7]");
            await Console.Out.WriteLineAsync("\tDelete a record by day [8]");
            await Console.Out.WriteLineAsync("\tAdd vacation [9]");
            await Console.Out.WriteLineAsync("\tDelete vacation [10]");
            await Console.Out.WriteLineAsync("\tShow your work by month[11]");
            await Console.Out.WriteLineAsync("\tRead work time from file [12]");
            await Console.Out.WriteLineAsync("\tPrint possible actions [13]");
            await Console.Out.WriteLineAsync("\tQuit [14]");
        }

        private static async Task RunAsync()
        {
            while (true)
            {
                await Console.Out.WriteLineAsync("Choose a number according to what you want to do: ");
                var selection = await Console.In.ReadLineAsync();
                switch (selection)
                {
                    case "1":
                        await Console.Out.WriteLineAsync($"Name: {Configuration.Name}");
                        await Console.Out.WriteLineAsync($"Email: {Configuration.Email}");
                        await Console.Out.WriteLineAsync($"Password: {Configuration.Password}");
                        break;
                    case "2":
                        await Console.Out.WriteLineAsync("What do you wish to update? [email|password|name]");
                        var update = await Console.In.ReadLineAsync();
                        await handler.HandleUpdateAsync(update);
                        break;
                    case "3":
                        await handler.HandleDeleteAsync();
                        break;
                    case "4":
                        await Console.Out.WriteLineAsync("Creating a new time record");
                        await handler.HandleCreateRecordAsync();
                        break;
                    case "5":
                        await Console.Out.WriteLineAsync("Choose a day (e.g. 31/5/2021): ");
                        var day = await Console.In.ReadLineAsync();
                        await handler.HandleReadRecordAsync(day);
                        break;
                    case "6":
                        await Console.Out.WriteLineAsync("Choose a record (e.g. 1): ");
                        var id = await Console.In.ReadLineAsync();
                        await handler.HandleUpdateRecordAsync(id);
                        break;
                    case "7":
                        await Console.Out.WriteLineAsync("Choose a record (e.g. 1): ");
                        var idDelete = await Console.In.ReadLineAsync();
                        await handler.HandleDeleteRecordAsync(idDelete);
                        break;
                    case "8":
                        await Console.Out.WriteLineAsync("Choose a day (e.g. 31/5/2021): ");
                        var dayDelete = await Console.In.ReadLineAsync();
                        DateTime oDate = Convert.ToDateTime(dayDelete);
                        await handler.HandleDeleteRecordAsync(oDate);
                        break;
                    case "9":
                        await Console.Out.WriteLineAsync("Choose a day (e.g. 31/5/2021): ");
                        var vacationDayString = await Console.In.ReadLineAsync();
                        DateTime vacationDay = Convert.ToDateTime(vacationDayString);
                        await handler.HandleAddVacationAsync(vacationDay);
                        break;
                    case "10":
                        await Console.Out.WriteLineAsync("Choose a day (e.g. 31/5/2021): ");
                        var vacationToDeleteString = await Console.In.ReadLineAsync();
                        DateTime vacationDayToDelete = Convert.ToDateTime(vacationToDeleteString);
                        await handler.HandleDeleteVacationAsync(vacationDayToDelete);
                        break;
                    case "11":
                        await Console.Out.WriteLineAsync("Choose a month (e.g. 5): ");
                        var month = await Console.In.ReadLineAsync();
                        await Console.Out.WriteLineAsync("Choose a year (e.g. 2021): ");
                        var year = await Console.In.ReadLineAsync();
                        await handler.HandleShowHistoryAsync(month, year);
                        break;
                    case "12":
                        break;
                    case "13":
                        await ShowActionsAsync();
                        break;
                    case "14":
                        Environment.Exit(0);
                        return;
                }
            }
        }

        private static async Task CheckPasswordAsync()
        {
            if (Configuration.Password == null)
            {
                await Console.Out.WriteLineAsync("Type your password: ");
                var password = await Console.In.ReadLineAsync();
                Configuration.Password = password;
            }
            else
            {
                await Console.Out.WriteLineAsync(
                    $"Using password from config file for user with email {Configuration.Email}");
                await Console.Out.WriteLineAsync("Do you wish to use this password? [y/n]");
                var response = await Console.In.ReadLineAsync();
                if (response != null && response.ToLower() == "n")
                {
                    await Console.Out.WriteLineAsync("Type your password: ");
                    var password = await Console.In.ReadLineAsync();
                    Configuration.Password = password;
                }

                if (response != null && response.ToLower() != "y" && response.ToLower() != "n")
                {
                    await Console.Error.WriteLineAsync("Bad choice");
                    Environment.Exit(1);
                }
            }
        }

        private static async Task CheckEmailAsync()
        {
            if (Configuration.Email == null)
            {
                await Console.Out.WriteLineAsync("Type your email: ");
                var email = await Console.In.ReadLineAsync();
                Configuration.Email = email;
            }
            else
            {
                await Console.Out.WriteLineAsync($"Using email {Configuration.Email} from config file");
                await Console.Out.WriteLineAsync("Do you wish to use this email? [y/n]");
                var response = await Console.In.ReadLineAsync();
                if (response != null && response.ToLower() == "n")
                {
                    await Console.Out.WriteLineAsync("Type your email: ");
                    var email = await Console.In.ReadLineAsync();
                    Configuration.Email = email;
                }

                if (response != null && response.ToLower() != "y" && response.ToLower() != "n")
                {
                    await Console.Error.WriteLineAsync("Bad choice");
                    Environment.Exit(1);
                }
            }
        }

        private static async Task ReadConfigAsync()
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
    }
}