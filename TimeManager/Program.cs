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
            await Console.Out.WriteLineAsync("Do you wish to login or register? [login/register]");
            var response = await Console.In.ReadLineAsync();
            switch (response)
            {
                case "register":
                    await Console.Out.WriteLineAsync("Creating a new user");
                    await handler.HandleCreateAsync();
                    break;
                case "login":
                    await readConfigAsync();
                    await checkEmailAsync();
                    await checkPasswordAsync();
                    var name = await handler.HandleLogicAsync(Configuration.Email, Configuration.Password);
                    if (name == null)
                    {
                        await Console.Error.WriteLineAsync("Bad credentials");
                        Environment.Exit(1);
                    }
                    await Console.Out.WriteLineAsync($"Welcome {name}!");
                    break;
            }

            await Console.Out.WriteLineAsync("Possible actions:");
            await Console.Out.WriteLineAsync("\tRead user settings [1]");
            await Console.Out.WriteLineAsync("\tUpdate user settings [2]");
            await Console.Out.WriteLineAsync("\tDelete user account [3]");
            await Console.Out.WriteLineAsync("\tCreate a new time record [4]");
            await Console.Out.WriteLineAsync("\tRead records [5]");
            await Console.Out.WriteLineAsync("\tUpdate a record [6]");
            await Console.Out.WriteLineAsync("\tDelete a record [7]");
            await Console.Out.WriteLineAsync("\tShow statistics [8]");
            await Console.Out.WriteLineAsync("\tPrint help [9]");

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
                        await Console.Out.WriteLineAsync("What do you wish to update? [email/password/name]");
                        var update = await Console.In.ReadLineAsync();
                        await handler.HandleUpdateAsync(update);
                        break;
                    case "3":
                        await handler.HandleDeleteAsync();
                        break;
                }

                break;
            }
        }

        private static async Task checkPasswordAsync()
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

        private static async Task checkEmailAsync()
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

        private static async Task readConfigAsync()
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