using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DAL.models;
using DAL.repository;
using Newtonsoft.Json;
using TimeManager.configuration;

namespace TimeManager.logic
{
    public class UserLogic
    {
        private readonly UserRepository userRepository = new();

        public async Task CreateUserAsync(string name, string email, string password)
        {
            var user = await userRepository.CreateUserAsync(name, email, password);
            if (user == null)
            {
                await Console.Error.WriteLineAsync("It wasn't possible to create a new user account");
            }
            else
            {
                await Console.Out.WriteLineAsync($"Welcome {user.Name}!");
            }
        }

        private static async Task writeToFileAsync(string contentToWrite, Stream stream)
        {
            var data = new UTF8Encoding(true).GetBytes(contentToWrite);
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task<User> ReadUserSettingsAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            return user;
        }
        
        public async Task<bool> CheckPasswordAsync(string email, string password)
        {
            try
            {
                var user = await userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return false;
                }

                return user.Password == password;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        public async Task UpdatePasswordAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            await Console.Out.WriteAsync("Type your new password: ");
            var newPassword = Console.ReadLine();
            if (newPassword is {Length: <= 3})
            {
                await Console.Error.WriteLineAsync("Too short password!");
                return;
            }
            await userRepository.InsertNewPasswordAsync(user, newPassword);
            Program.Configuration.Password = newPassword;
            
            await Console.Out.WriteLineAsync("Do you wish to update the config.json file? [y/n]");
            var response = await Console.In.ReadLineAsync();
            if (response != null && response.ToLower() == "y")
            {
                await updateJsonFileAsync(newPassword, null, null);
            }
        }
        
        public async Task UpdateNameAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            await Console.Out.WriteAsync("Type your new password: ");
            var newName = Console.ReadLine();
            if (newName is {Length: <= 3})
            {
                await Console.Error.WriteLineAsync("Too short name!");
                return;
            }
            await userRepository.InsertNewNameAsync(user, newName);
            Program.Configuration.Name = newName;
            
            await Console.Out.WriteLineAsync("Do you wish to update the config.json file? [y/n]");
            var response = await Console.In.ReadLineAsync();
            if (response != null && response.ToLower() == "y")
            {
                await updateJsonFileAsync(null, null, newName);
            }
        }

        private async Task updateJsonFileAsync(string newPassword, string newEmail, string newName)
        {
            string configFile =
                Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../configuration/"),
                    "config.json");
            await File.WriteAllTextAsync(configFile, String.Empty);
            await using var stream = File.OpenWrite(configFile);
            await writeToFileAsync(string.Empty, stream);

            Configuration newConfiguration = new Configuration
            {
                Password = newPassword ?? Program.Configuration.Password,
                Email = newEmail ?? Program.Configuration.Email,
                Name = newName ?? Program.Configuration.Name
            };
            string output = JsonConvert.SerializeObject(newConfiguration, Formatting.Indented);
            await writeToFileAsync(output, stream);
        }

        public async Task<bool> DeleteUserAsync(string email)
        {
            var success = await userRepository.DeleteUserAsync(email);
            if (success)
            {
                await Console.Out.WriteLineAsync("User account deleted");
                return true;
            }
            await Console.Error.WriteLineAsync("It wasn't possible to delete the account");
            return false;
        }

        public async Task UpdateEmailAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(Program.Configuration.Email);
            await Console.Out.WriteAsync("Type your new email: ");
            var newEmail = await Console.In.ReadLineAsync();
            await userRepository.InsertNewEmailAsync(user, newEmail);
            Program.Configuration.Email = newEmail;
            
            await Console.Out.WriteLineAsync("Do you wish to update the config.json file? [y/n]");
            var response = await Console.In.ReadLineAsync();
            if (response != null && response.ToLower() == "y")
            {
                await updateJsonFileAsync(null, newEmail, null);
            }
        }
    }
}