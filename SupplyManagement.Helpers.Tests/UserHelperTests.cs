using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SupplyManagement.Models;
using System.Security.Claims;

namespace SupplyManagement.Helpers.Tests
{
    [TestClass]
    public class UserHelperTests
    {
        [TestMethod]
        public async Task GetCurrentUserAsync_ThrowsException_WhenCurrentUserIsNull()
        {
            // Arrange
            var store = new Mock<IUserStore<IdentityUser>>();
            var userManager = new UserManager<IdentityUser>(store.Object, null, null, null, null, null, null, null, null);
            var loggerMock = new Mock<ILogger<UserHelper>>();
            var userHelper = new UserHelper(userManager, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => userHelper.GetCurrentUserAsync(null));
        }

        [TestMethod]
        public async Task GetCurrentUserAsync_ThrowsException_WhenUserIdIsNull()
        {
            // Arrange
            var store = new Mock<IUserStore<IdentityUser>>();
            var userManager = new UserManager<IdentityUser>(store.Object, null, null, null, null, null, null, null, null);
            var loggerMock = new Mock<ILogger<UserHelper>>();
            var userHelper = new UserHelper(userManager, loggerMock.Object);
            var currentUser = new ClaimsPrincipal();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => userHelper.GetCurrentUserAsync(currentUser));
        }

        [TestMethod]
        public async Task GetCurrentUserAsync_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            var currentUser = new ClaimsPrincipal();
            var user = new IdentityUser { Id = "1" };
            const string userId = "user123";
            var userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            userManagerMock.Setup(u => u.GetUserId(currentUser)).Returns(userId);
            var loggerMock = new Mock<ILogger<UserHelper>>();
            var userHelper = new UserHelper(userManagerMock.Object, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => userHelper.GetCurrentUserAsync(currentUser));
        }

        [TestMethod]
        public async Task GetCurrentUserAsync_ReturnsUserIdAndUser_WhenValidUserFound()
        {
            // <IdArrange
            var currentUser = new ClaimsPrincipal();
            const string userId = "user123";
            var user = new User { Id = userId };
            var userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            userManagerMock.Setup(u => u.GetUserId(currentUser)).Returns(userId);
            userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            var loggerMock = new Mock<ILogger<UserHelper>>();
            var userHelper = new UserHelper(userManagerMock.Object, loggerMock.Object);

            // Act
            var (returnedUserId, returnedUser) = await userHelper.GetCurrentUserAsync(currentUser);

            // Assert
            Assert.AreEqual(userId, returnedUserId);
            Assert.AreEqual(user, returnedUser);
        }
    }
}
