using AuthManager.Contexts;
using AuthManager.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthManager.Services.UserService
{
    public class UserService(
            [FromServices] DbAuthContext dbAuthContext
        ): IUserService
    {
        private readonly DbAuthContext _dbAuthContext = dbAuthContext;
        public async Task<User?> GetByGuid(Guid id)
        {
            return await this._dbAuthContext.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmail(string Email)
        {
            return await this._dbAuthContext.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.EMail.Equals(Email));
        }
        public async Task<User> Add(User user)
        {
            await this._dbAuthContext.AddAsync(user);
            await this._dbAuthContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> Update(User user)
        {
            this._dbAuthContext.Update(user);
            await this._dbAuthContext.SaveChangesAsync();
            return user;
        }

        public async Task Delete(Guid guid)
        {
            User? user = await this.GetByGuid(guid);
            if (user == null)
                return;
            this._dbAuthContext.Remove(user);
            await this._dbAuthContext.SaveChangesAsync();
        }
    }
}
