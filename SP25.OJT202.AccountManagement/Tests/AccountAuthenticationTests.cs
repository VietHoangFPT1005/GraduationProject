using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SP25.OJT202.AccountManagement.Application;
using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Domain.Entities.Response;

namespace SP25.OJT202.AccountManagement.Extend.Tests
{
    [TestFixture]
    public class AccountAuthenticationTests
    {
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private AccountSecurityService _accountAuthentication;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<User>>();
            var userValidatorsMock = new List<IUserValidator<User>> { new Mock<IUserValidator<User>>().Object };
            var passwordValidatorsMock = new List<IPasswordValidator<User>> { new Mock<IPasswordValidator<User>>().Object };
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorsMock = new Mock<IdentityErrorDescriber>();
            var servicesMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<UserManager<User>>>();

            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidatorsMock.ToArray(),
                passwordValidatorsMock.ToArray(),
                keyNormalizerMock.Object,
                errorsMock.Object,
                servicesMock.Object,
                loggerMock.Object
            );

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleValidatorsMock = new List<IRoleValidator<IdentityRole>> { new Mock<IRoleValidator<IdentityRole>>().Object };
            var roleKeyNormalizerMock = new Mock<ILookupNormalizer>();
            var roleErrorsMock = new Mock<IdentityErrorDescriber>();
            var roleLoggerMock = new Mock<ILogger<RoleManager<IdentityRole>>>();

            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object,
                roleValidatorsMock.ToArray(),
                roleKeyNormalizerMock.Object,
                roleErrorsMock.Object,
                roleLoggerMock.Object
            );

            _configurationMock = new Mock<IConfiguration>();

            _accountAuthentication = new AccountSecurityService(_userManagerMock.Object, _configurationMock.Object, _roleManagerMock.Object);
        }


        [Test]
        public async Task SignUpStudentAsync_ShouldReturnSucceededResponse_WhenUserCreationIsSuccessful()
        {
            // Arrange
            var student = new Student
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                PhoneNumber = "1234567890",
                Address = "123 Test St"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountAuthentication.SignUpStudentAsync(student);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Status, Is.EqualTo("Succeeded"));
            Assert.That(result.Message, Is.EqualTo("Sign up student successfully."));
        }

        [Test]
        public async Task SignInAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var loginAccount = new AccountLogin
            {
                Email = "nonexistentuser@example.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _accountAuthentication.SignInAsync(loginAccount);

            // Assert
            Assert.IsNull(result);
        }
    }
}
