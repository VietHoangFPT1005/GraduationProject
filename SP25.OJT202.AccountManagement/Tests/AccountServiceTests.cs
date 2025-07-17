using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SP25.OJT202.AccountManagement.Application;
using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Domain.Entities.Response;
using SP25.OJT202.AccountManagement.Infrastructure;
using Assert = NUnit.Framework.Assert;

namespace SP25.OJT202.AccountManagement.Extend.Tests
{
    /// <summary>
    /// Unit test for account service operations.
    /// </summary>
    [TestFixture]
    public class AccountServiceTests
    {
        private Mock<IAccountRepository> _repoMock;
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private AccountService _accountService;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IAccountRepository>();
            _userManagerMock = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);
            _configurationMock = new Mock<IConfiguration>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object,
                new IRoleValidator<IdentityRole>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object);
            _accountService = new AccountService(_repoMock.Object, _userManagerMock.Object);
        }

        [Test]
        public async Task CreateTeacherAccountAsync_ShouldReturnStatusResponse()
        {
            // Arrange
            var teacher = new Teacher { Email = "teacher@example.com", Password = "password" };
            _repoMock.Setup(x => x.CreateAccountAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.CreateTeacherAccountAsync(teacher);

            // Assert
            Assert.That(result?.Status, Is.EqualTo("Succeeded"));
        }

        [Test]
        public async Task UpdateAccountAsync_AccountFound_ReturnsSucceededStatus()
        {
            // Arrange
            var accountModification = new UserModification
            {
                Email = "test@example.com",
                UserName = "TestUser",
                PhoneNumber = "1234567890",
                Salary = 50000,
                Balance = 1000,
                Address = "Test Address"
            };

            var existingUser = new User
            {
                Email = "test@example.com",
                UserName = "OldUser",
                PhoneNumber = "0987654321",
                Salary = 40000,
                Balance = 500,
                Address = "Old Address"
            };
            var existingAccountResponse = new ObjectResponse { User = existingUser };

            _repoMock.Setup(repo => repo.GetAccountsAsync()).ReturnsAsync(new List<User> { existingUser });
            _repoMock.Setup(repo => repo.UpdateAccountAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _repoMock.Setup(repo => repo.GetAccountsAsync()).ReturnsAsync(new List<User> { existingUser });

            // Act
            var result = await _accountService.UpdateAccountAsync(accountModification);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Status, Is.EqualTo("Succeeded"));
            Assert.That(result.Message, Is.EqualTo("Update account successfully."));
        }

        [Test]
        public async Task FindAccountByIdAsync_ShouldReturnUser()
        {
            // Arrange
            var user = new User { Id = "1" };
            _repoMock.Setup(x => x.GetAccountsAsync()).ReturnsAsync(new ListResponse() { List = new List<User>() { user } }.List);

            // Act
            var result = await _accountService.FindAccountByIdAsync(user);

            // Assert
            Assert.That(result?.User, Is.EqualTo(user));
        }

        [Test]
        public async Task FindAccountByEmailAsync_ShouldReturnUser()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };
            _repoMock.Setup(x => x.GetAccountsAsync()).ReturnsAsync(new ListResponse() { List = new List<User>() { user } }.List);

            // Act
            var result = await _accountService.FindAccountByEmailAsync(user);

            // Assert
            Assert.That(result?.User, Is.EqualTo(user));
        }

        [Test]
        public async Task GetAccountsAsync_ShouldReturnListOfUsers()
        {
            // Arrange
            var users = new List<User> { new User { Email = "test@example.com" } };
            _repoMock.Setup(x => x.GetAccountsAsync()).ReturnsAsync(new ListResponse() { List = users }.List);

            // Act
            var result = await _accountService.GetAccountsAsync();

            // Assert
            Assert.That(result?.List, Is.EqualTo(users));
        }

        [Test]
        public async Task GetStudentAccountsAsync_ShouldReturnListOfStudents()
        {
            // Arrange
            var users = new List<User> { new User { Email = "student@example.com" } };
            _repoMock.Setup(x => x.GetAccountsAsync()).ReturnsAsync(new ListResponse() { List = users }.List);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { ApplicationRoles.Student });

            // Act
            var result = await _accountService.GetStudentAccountsAsync();

            // Assert
            Assert.That(result?.List?.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAccountsByRoleAsync_ShouldReturnListOfUsersWithRole()
        {
            // Arrange
            var users = new List<User> { new User { Email = "user@example.com" } };
            _repoMock.Setup(x => x.GetAccountsAsync()).ReturnsAsync(new ListResponse() { List = users }.List);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "Role" });

            // Act
            var result = await _accountService.GetAccountsByRoleAsync("Role");

            // Assert
            Assert.That(result?.List?.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteAccountAsync_ShouldReturnIdentityResult()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };
            _repoMock.Setup(x => x.DeleteAccountAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.DeleteAccountAsync(user);

            // Assert
            Assert.True(result?.Status == "Succeeded");
        }
    }
}
