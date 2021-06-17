using System;
using System.Threading.Tasks;
using TimeManager.logic;


namespace TimeManager
{
    static class Program
    {
        private static readonly Handler handler = new();

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            await Console.Out.WriteAsync("Do you wish to login or register [login|register]: ");
            var response = await Console.In.ReadLineAsync();
            await handler.ReadConfigAsync();
            switch (response)
            {
                case "register":
                    await Console.Out.WriteLineAsync("Creating a new user");
                    await handler.HandleCreateAsync();
                    break;
                case "login":
                    await CheckEmailAsync();
                    await CheckPasswordAsync();
                    var name = await handler.HandleLogicAsync(handler.Configuration.Email,
                        handler.Configuration.Password);
                    if (name == null)
                    {
                        await Console.Error.WriteLineAsync("Bad credentials");
                        Environment.Exit(1);
                    }

                    await Console.Out.WriteLineAsync($"Welcome {name}!");
                    break;
                default:
                    await Console.Error.WriteLineAsync("Unknown command");
                    Environment.Exit(1);
                    return;
            }

            await ShowActionsAsync();
            await RunAsync();
        }

        private static async Task ShowActionsAsync()
        {
            await Console.Out.WriteLineAsync("Possible actions [1-17]:");
            await Console.Out.WriteAsync("\tRead user settings [1]\n");
            await Console.Out.WriteAsync("\tUpdate user settings [2]\n");
            await Console.Out.WriteAsync("\tDelete user account [3]\n");
            await Console.Out.WriteAsync("\tCreate a new time record [4]\n");
            await Console.Out.WriteAsync("\tRead records by day [5]\n");
            await Console.Out.WriteAsync("\tUpdate a record by id [6]\n");
            await Console.Out.WriteAsync("\tDelete a record by id [7]\n");
            await Console.Out.WriteAsync("\tDelete a record by day [8]\n");
            await Console.Out.WriteAsync("\tAdd a break [9]\n");
            await Console.Out.WriteAsync("\tUpdate a break by date [10]\n");
            await Console.Out.WriteAsync("\tDelete a break by date [11]\n");
            await Console.Out.WriteAsync("\tAdd a vacation [12]\n");
            await Console.Out.WriteAsync("\tDelete a vacation [13]\n");
            await Console.Out.WriteAsync("\tShow your work by month [14]\n");
            await Console.Out.WriteAsync("\tRead work time from file by month [15]\n");
            await Console.Out.WriteAsync("\tPrint possible actions [16]\n");
            await Console.Out.WriteAsync("\tQuit [17]\n");
        }

        private static async Task RunAsync()
        {
            while (true)
            {
                await Console.Out.WriteAsync("Choose a number according to what you want to do: ");
                var selection = await Console.In.ReadLineAsync();
                switch (selection)
                {
                    case "1":
                        await Console.Out.WriteLineAsync($"Name: {handler.Configuration.Name}");
                        await Console.Out.WriteLineAsync($"Email: {handler.Configuration.Email}");
                        await Console.Out.WriteLineAsync($"Password: {handler.Configuration.Password}");
                        if (handler.Configuration.ChatLog != null)
                        {
                            await Console.Out.WriteLineAsync($"Path to chat log: {handler.Configuration.ChatLog}");
                        }
                        break;
                    case "2":
                        await Console.Out.WriteAsync("What do you wish to update [password|name]: ");
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
                        await Console.Out.WriteAsync("Choose a day (e.g. 31/5/2021): ");
                        var day = await Console.In.ReadLineAsync();
                        handler.HandleReadRecord(day);
                        break;
                    case "6":
                        await Console.Out.WriteAsync("Choose a record (e.g. 1): ");
                        var id = await Console.In.ReadLineAsync();
                        await handler.HandleUpdateRecordAsync(id);
                        break;
                    case "7":
                        await Console.Out.WriteAsync("Choose a record (e.g. 1): ");
                        var idDelete = await Console.In.ReadLineAsync();
                        await handler.HandleDeleteRecordAsync(idDelete);
                        break;
                    case "8":
                        await Console.Out.WriteAsync("Choose a day (e.g. 31/5/2021): ");
                        var dayDelete = await Console.In.ReadLineAsync();
                        await handler.HandleDeleteRecordAsync(dayDelete);
                        break;
                    case "9":
                        await Console.Out.WriteLineAsync("Creating a new break");
                        await handler.HandleAddBreakAsync();
                        break;
                    case "10":
                        await handler.HandleUpdateBreakAsync();
                        break;
                    case "11":
                        await handler.HandleDeleteBreakAsync();
                        break;
                    case "12":
                        await Console.Out.WriteAsync("Choose a day (e.g. 31/5/2021): ");
                        var vacationDayString = await Console.In.ReadLineAsync();
                        await handler.HandleAddVacationAsync(vacationDayString);
                        break;
                    case "13":
                        await Console.Out.WriteAsync("Choose a day (e.g. 31/5/2021): ");
                        var vacationDayToDelete = await Console.In.ReadLineAsync();
                        await handler.HandleDeleteVacationAsync(vacationDayToDelete);
                        break;
                    case "14":
                        await Console.Out.WriteAsync("Choose a month (e.g. 5): ");
                        var month = await Console.In.ReadLineAsync();
                        await Console.Out.WriteAsync("Choose a year (e.g. 2021): ");
                        var year = await Console.In.ReadLineAsync();
                        handler.HandleShowHistory(month, year);
                        break;
                    case "15":
                        await Console.Out.WriteAsync("Choose a month (e.g. 5): ");
                        var fromFileMonth = await Console.In.ReadLineAsync();
                        await Console.Out.WriteAsync("Choose a year (e.g. 2021): ");
                        var fromFileYear = await Console.In.ReadLineAsync();
                        await handler.HandleReadFromFileAsync(fromFileMonth, fromFileYear);
                        break;
                    case "16":
                        await ShowActionsAsync();
                        break;
                    case "17":
                        Environment.Exit(0);
                        return;
                    default:
                        await Console.Error.WriteLineAsync("Unknown command");
                        break;
                }
            }
        }

        private static async Task CheckPasswordAsync()
        {
            if (handler.Configuration.Password == null)
            {
                await Console.Out.WriteAsync("Type your password: ");
                var password = await Console.In.ReadLineAsync();
                handler.Configuration.Password = password;
            }
            else
            {
                await Console.Out.WriteLineAsync(
                    $"Using password from config file for user with email {handler.Configuration.Email}");
                await Console.Out.WriteAsync("Do you wish to use this password [y/n]: ");
                var response = await Console.In.ReadLineAsync();
                if (response != null && response.ToLower() == "n")
                {
                    await Console.Out.WriteAsync("Type your password: ");
                    var password = await Console.In.ReadLineAsync();
                    handler.Configuration.Password = password;
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
            if (handler.Configuration.Email == null)
            {
                await Console.Out.WriteAsync("Type your email: ");
                var email = await Console.In.ReadLineAsync();
                handler.Configuration.Email = email;
            }
            else
            {
                await Console.Out.WriteLineAsync($"Using email {handler.Configuration.Email} from config file");
                await Console.Out.WriteAsync("Do you wish to use this email [y/n]: ");
                var response = await Console.In.ReadLineAsync();
                if (response != null && response.ToLower() == "n")
                {
                    await Console.Out.WriteAsync("Type your email: ");
                    var email = await Console.In.ReadLineAsync();
                    handler.Configuration.Email = email;
                }

                if (response != null && response.ToLower() != "y" && response.ToLower() != "n")
                {
                    await Console.Error.WriteLineAsync("Bad choice");
                    Environment.Exit(1);
                }
            }

            var user = await handler.HandleReadUsersByEmailAsync(handler.Configuration.Email);
            if (user == null)
            {
                handler.Configuration.Password = null;
                return;
            }

            if (user.Password != handler.Configuration.Password)
            {
                handler.Configuration.Password = null;
            }
        }
    }
}