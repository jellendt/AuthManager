using AuthManager.DependecyInjection;
using AuthManager.Entities;

namespace AuthManager.Services.UserService
{
    public interface IUserService : IScopedDependency
    {
        Task<User?> GetByGuid(Guid id);
        Task<User?> GetByEmail(string eMail);
        Task<bool> CheckEmailExists(string email);
        Task<bool> CheckUsernameExists(string username);
        Task<User> Add(User user);
        Task<User> Update(User user);
        Task Delete(Guid guid);
    }
}