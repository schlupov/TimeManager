using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.models;
using Microsoft.EntityFrameworkCore;

namespace DAL.repository
{
    public class UserRepository : GenericRepository<User>
    {
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await GetByEmailAsync(email);
        }

        public async Task<User> CreateUserAsync(string name, string email, string password)
        {
            User user = new User {Name = name, Email = email, Password = password};
            try
            {
                await InsertAsync(user);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return user;
        }

        public async Task InsertNewPasswordAsync(User user, string password)
        {
            user.Password = password;
            await SaveAsync();
        }
        
        public async Task InsertNewEmailAsync(User user, string email)
        {
            user.Email = email;
            await SaveAsync();
        }
        
        public async Task<bool> DeleteUserAsync(string email)
        {
            try
            {
                await DeleteAsync(email);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        public async Task InsertNewNameAsync(User user, string name)
        {
            user.Name = name;
            await SaveAsync();
        }
    }
}