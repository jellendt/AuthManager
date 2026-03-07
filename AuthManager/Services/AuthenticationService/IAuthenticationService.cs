using AuthManager.DependecyInjection;
using AuthManager.Entities;

namespace AuthManager.Services.AuthenticationService
{
    public interface IAuthenticationService : IScopedDependency
    {
        User CreateUser(string username, string eMail, string password);
        bool CheckPassword(User user, string password);
    }
}
