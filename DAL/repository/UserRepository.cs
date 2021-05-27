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
            var token = await GenerateNewTokenAsync();
            User user = new User {Name = name, Email = email, Password = password, Token = token};
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

        public async Task<string> GenerateNewTokenAsync()
        {
            var users = await GetAllAsync();
            List<string> tokens = users.Select(userFromDb => userFromDb.Token).ToList();
            string token;
            while (true)
            {
                token = Guid.NewGuid().ToString("N");
                if (!tokens.Contains(token))
                {
                    break;
                }
            }
            return token;
        }

        public async Task InsertNewTokenAsync(User user, string token)
        {
            user.Token = token;
            await SaveAsync();
        }
        
        public async Task InsertNewPasswordAsync(User user, string password)
        {
            user.Password = password;
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

        public async Task<User> GetUserByTokenAsync(string token)
        {
            return await context.Users.FirstOrDefaultAsync(x => x.Token == token);
        }
    }
}