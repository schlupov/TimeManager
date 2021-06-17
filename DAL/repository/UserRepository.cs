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
            return await GetByPropertyAsync(email);
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

        public async Task AddWorkToUserAsync(string email, Work work)
        {
            var userFromDb = context.Users.First(x => x.Email == email);
            userFromDb.Work.Add(work);
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
                //await DeleteUserRecords(email);
                await DeleteAsync(email);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        private async Task DeleteUserRecords(string email)
        {
            // TODO: opravit, to prijde do jednotlivych repository
            var desiredWork = context.Work.Where(x => x.UserEmail == email).ToList();
            var desiredBreak = context.Break.Where(x => x.UserEmail == email).ToList();
            var desiredVacation = context.Vacation.Where(x => x.UserEmail == email).ToList();
            foreach (var w in desiredWork)
            {
                await DeleteAsync(w.Id);
            }

            foreach (var b in desiredBreak)
            {
                await DeleteAsync(b.Id);
            }

            foreach (var v in desiredVacation)
            {
                await DeleteAsync(v.Id);
            }
        }

        public async Task InsertNewNameAsync(User user, string name)
        {
            user.Name = name;
            await SaveAsync();
        }

        public async Task AddBreakToUserAsync(string email, Break bBreak)
        {
            var userFromDb = context.Users.First(x => x.Email == email);
            userFromDb.Break.Add(bBreak);
            await SaveAsync();
        }

        public async Task AddVacationToUserAsync(string email, Vacation vacation)
        {
            var userFromDb = context.Users.First(x => x.Email == email);
            userFromDb.Vacation.Add(vacation);
            await SaveAsync();
        }
    }
}