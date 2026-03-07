using AuthManager.Contexts;
using AuthManager.Entities;
using AuthManager.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthManager.Services.UserService
{
    public class UserService(
            [FromServices] DbAuthContext dbAuthContext
        ): IUserService
    {
        public async Task<User?> GetByGuid(Guid id)
        {
            return await dbAuthContext.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmail(string Email)
        {
            return await dbAuthContext.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.EMail.Equals(Email));
        }
        public async Task<User> Add(User user)
        {
            await dbAuthContext.AddAsync(user);
            await dbAuthContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> Update(User user)
        {
            dbAuthContext.Update(user);
            await dbAuthContext.SaveChangesAsync();
            return user;
        }

        public async Task Delete(Guid guid)
        {
            User user = await GetByGuid(guid) ?? throw new UserNotFoundException();
            dbAuthContext.Remove(user);
            await dbAuthContext.SaveChangesAsync();
        }

        public Task<bool> CheckEmailExists(string email)
        {
            return dbAuthContext.Users.AnyAsync(u => u.EMail == email);
        }

        public Task<bool> CheckUsernameExists(string username)
        {
            return dbAuthContext.Users.AnyAsync(u => u.Username == username);
        }
    }
}
