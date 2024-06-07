using AuthManager.Entities;

namespace AuthManager.Services.UserService
{
    public interface IUserService
    {
        Task<User?> GetByGuid(Guid id);
        Task<User?> GetByEmail(string eMail);
        Task<User> Add(User user);
        Task<User> Update(User user);
        Task Delete(Guid guid);
    }
}