using AuthManager.Services.UserService;
using Microsoft.Extensions.DependencyInjection;

namespace AuthManager_Test
{
    public class UserService_Test
        (
            TestFixture testFixture
        ) : IClassFixture<TestFixture>
    {
        private readonly IUserService _userService = testFixture.CreateUnitUnderTest<IUserService>();

        [Fact]
        public void Delete_ThrowError_WhenUserNotExists()
        {
            //Arange
            _userService.

            //Act

            //Assert
        }
    }
}