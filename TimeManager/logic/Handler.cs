using System;
using System.Globalization;
using System.Threading.Tasks;
using DAL;

namespace TimeManager.logic
{
    public class Handler
    {
        private readonly UserLogic userLogic = new();

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

        public async Task HandleRecordAsync(string type, string inTime, string outTime, string date, string comment)
        {
            if (string.IsNullOrEmpty(type))
            {
                type = WorkType.Issue.ToString("G");
            }
            await Console.Out.WriteLineAsync(type);
            var dateTime = DateTime.Parse(date, new CultureInfo("cs-CZ"));
            var intt = DateTime.Parse(inTime, new CultureInfo("cs-CZ"));
            Console.WriteLine(dateTime.ToShortDateString());
            Console.WriteLine(intt.Hour);
        }
    }
}