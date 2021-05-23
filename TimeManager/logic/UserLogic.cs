using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
                await Console.Out.WriteLineAsync(
                    $"Your token is {user.Token}, " +
                    $"token saved to {Path.Combine(Program.Configuration.PathToToken, user.Email)}");
                var tokenFile = Path.Combine(Program.Configuration.PathToToken, user.Email);
                try
                {
                    await using var stream = File.Create(tokenFile);
                    await writeToFileAsync(user.Token, stream);
                }
                catch (DirectoryNotFoundException)
                {
                    await Console.Error.WriteLineAsync(
                        $"It wasn't possible to create your token file into " +
                        $"{Program.Configuration.PathToToken}, verify that the directory exists");
                }
            }
        }

        private static async Task writeToFileAsync(string contentToWrite, Stream stream)
        {
            var data = new UTF8Encoding(true).GetBytes(contentToWrite);
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task ReadUserSettingsAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                await Console.Error.WriteLineAsync($"No such user!");
            }
            else
            {
                await Console.Out.WriteLineAsync($"Name: {user.Name}\nEmail: {user.Email}\nPassword: {user.Password}");
            }
        }

        public async Task ReadUserTokenAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                await Console.Error.WriteLineAsync("No such user!");
            }
            else
            {
                await Console.Out.WriteLineAsync($"Token: {user.Token}");
            }
        }

        public async Task<bool> CheckTokenAsync(string email)
        {
            try
            {
                using var reader = File.OpenText(Path.Combine(Program.Configuration.PathToToken, email));
                var token = await reader.ReadToEndAsync();
                var user = await userRepository.GetUserByTokenAsync(token);
                return user != null;
            }
            catch (Exception ex)            
            {                
                if (ex is ArgumentNullException or DirectoryNotFoundException or FileNotFoundException)
                {
                    return false;
                }

                throw;
            }
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

        public async Task<string> GenerateNewTokenAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            var token = await userRepository.GenerateNewTokenAsync();
            await userRepository.InsertNewTokenAsync(user, token);
            return token;
        }

        public async Task<bool> UpdateTokenInFileAsync(string newToken, string email)
        {
            try
            {
                var tokenFile = Path.Combine(Program.Configuration.PathToToken, email);
                await File.WriteAllTextAsync(tokenFile, String.Empty);
                await using var stream = File.OpenWrite(tokenFile);
                await writeToFileAsync(newToken, stream);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        public async Task UpdatePasswordAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                await Console.Error.WriteLineAsync("No such user!");
                return;
            }

            await Console.Out.WriteAsync("Type your old password: ");
            var password = Console.ReadLine();
            if (user.Password != password)
            {
                await Console.Error.WriteLineAsync("Invalid password!");
                return;
            }

            await Console.Out.WriteAsync("Type your new password: ");
            var newPassword = Console.ReadLine();
            if (newPassword is {Length: < 4})
            {
                await Console.Error.WriteLineAsync("Too short password!");
                return;
            }

            await userRepository.InsertNewPasswordAsync(user, newPassword);
            if (Program.Configuration.Password != null)
            {
                await updateJsonFileAsync(newPassword);
            }
        }

        private async Task updateJsonFileAsync(string newPassword)
        {
            string configFile =
                Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../configuration/"),
                    "config.json");
            await File.WriteAllTextAsync(configFile, String.Empty);
            await using var stream = File.OpenWrite(configFile);
            await writeToFileAsync(string.Empty, stream);

            Configuration newConfiguration = new Configuration
            {
                PathToToken = Program.Configuration.PathToToken,
                Password = newPassword,
                Email = Program.Configuration.Email
            };
            string output = JsonConvert.SerializeObject(newConfiguration, Formatting.Indented);
            await writeToFileAsync(output, stream);
        }

        public async Task DeleteUserAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                await Console.Out.WriteAsync("Type your password for deleting your account: ");
                var password = Console.ReadLine();
                if (user.Password != password)
                {
                    await Console.Error.WriteLineAsync("Incorrect password!");
                    return;
                }

                var success = await userRepository.DeleteUserAsync(email, password);
                if (success)
                {
                    await Console.Out.WriteAsync($"User {user.Email} deleted. Don't forget to update your config.json");
                    try
                    {
                        File.Delete(Path.Combine(Program.Configuration.PathToToken, email));
                    }
                    catch (Exception ex)            
                    {                
                        if (ex is DirectoryNotFoundException or FileNotFoundException)
                        {
                            await Console.Error.WriteLineAsync(
                                $"Couldn't delete {Path.Combine(Program.Configuration.PathToToken, email)}");
                            return;
                        }

                        throw;
                    }

                    return;
                }
            }

            await Console.Error.WriteLineAsync("It wasn't possible to delete the account");
        }
    }
}