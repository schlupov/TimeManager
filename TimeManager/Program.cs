using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimeManager.configuration;
using System.CommandLine;
using System.CommandLine.Invocation;
using TimeManager.logic;


namespace TimeManager
{
    class Program
    {
        public static Configuration Configuration { get; set; }
        private static readonly Handler handler = new();

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            await readConfigAsync();

            ArgumentParser argumentParser = new ArgumentParser(
                new Command("create", "create new user account"),
                new Command("read", "read user account settings"),
                new Command("update", "update user account settings"),
                new Command("delete", "delete user account"));
            var create = argumentParser.CreateOptions();
            create.Handler = CommandHandler.Create<string, string, string>(handler.HandleCreateAsync);
            var read = argumentParser.ReadOptions();
            read.Handler = CommandHandler.Create<string, string, bool, bool>(handler.HandleReadAsync);
            var update = argumentParser.UpdateOptions();
            update.Handler = CommandHandler.Create<string, bool, bool, bool>(handler.HandleUpdateAsync);
            var delete = argumentParser.DeleteOptions();
            delete.Handler = CommandHandler.Create<string, bool>(handler.HandleDeleteAsync);

            var rootCommand = new RootCommand
            {
                create,
                read,
                update,
                delete
            };

            await rootCommand.InvokeAsync(args);
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
                    PathToToken = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../configuration/"),
                    Password = null, Email = null
                };
            }
        }
    }
}