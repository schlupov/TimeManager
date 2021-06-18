using System;
using System.Linq;
using System.Threading.Tasks;
using DAL.models;

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

        public async Task InsertNewPasswordAsync(User user, string password)
        {
            user.Password = password;
            await context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserAsync(string email)
        {
            try
            {
                var userFromDb = context.Users.First(x => x.Email == email);
                context.Users.Attach(userFromDb);
                await context.Entry(userFromDb).Collection(t => t.Work).LoadAsync();
                await context.Entry(userFromDb).Collection(t => t.Break).LoadAsync();
                await context.Entry(userFromDb).Collection(t => t.Vacation).LoadAsync();
                context.Remove(userFromDb);
                await context.SaveChangesAsync();
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
            await context.SaveChangesAsync();
        }

        public async Task AddWorkToUserAsync(string email, Work work)
        {
            var userFromDb = context.Users.First(x => x.Email == email);
            context.Users.Attach(userFromDb);
            await context.Entry(userFromDb).Collection(t => t.Work).LoadAsync();
            userFromDb.Work.Add(work);
            await context.SaveChangesAsync();
        }

        public async Task AddBreakToUserAsync(string email, Break bBreak)
        {
            var userFromDb = context.Users.First(x => x.Email == email);
            context.Users.Attach(userFromDb);
            await context.Entry(userFromDb).Collection(t => t.Break).LoadAsync();
            userFromDb.Break.Add(bBreak);
            await context.SaveChangesAsync();
        }

        public async Task AddVacationToUserAsync(string email, Vacation vacation)
        {
            var userFromDb = context.Users.First(x => x.Email == email);
            context.Users.Attach(userFromDb);
            await context.Entry(userFromDb).Collection(t => t.Vacation).LoadAsync();
            userFromDb.Vacation.Add(vacation);
            await context.SaveChangesAsync();
        }
    }
}