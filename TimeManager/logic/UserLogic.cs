using System;
using System.Threading.Tasks;
using DAL.models;
using DAL.repository;

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
        
        public async Task AddWorkToUserAsync(string email, Work work)
        {
            await userRepository.AddWorkToUserAsync(email, work);
        }
        
        public async Task AddBreakToUserAsync(string email, Break bBreak)
        {
            await userRepository.AddBreakToUserAsync(email, bBreak);
        }
        
        public async Task AddVacationToUserAsync(string email, Vacation vacation)
        {
            await userRepository.AddVacationToUserAsync(email, vacation);
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

        public async Task<string> UpdatePasswordAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            await Console.Out.WriteAsync("Type your new password: ");
            var newPassword = Console.ReadLine();
            if (newPassword is {Length: <= 3})
            {
                await Console.Error.WriteLineAsync("Too short password!");
                return null;
            }
            await userRepository.InsertNewPasswordAsync(user, newPassword);
            return newPassword;
        }
        
        public async Task<string> UpdateNameAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            await Console.Out.WriteAsync("Type your new name: ");
            var newName = Console.ReadLine();
            if (newName is {Length: <= 3})
            {
                await Console.Error.WriteLineAsync("Too short name!");
                return null;
            }
            await userRepository.InsertNewNameAsync(user, newName);
            return newName;
        }

        public async Task<bool> DeleteUserAsync(string email)
        {
            var success = await userRepository.DeleteUserAsync(email);
            if (success)
            {
                await Console.Out.WriteLineAsync("User account deleted");
                await Console.Out.WriteLineAsync("Don't forget to update the config.json file if you use it");
                return true;
            }
            await Console.Error.WriteLineAsync("It wasn't possible to delete the account");
            return false;
        }
    }
}