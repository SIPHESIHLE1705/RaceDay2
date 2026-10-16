
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Racetest




{
    public class UserSessionTests
    {
        [Fact]
        public void GetMyProfile_WithoutLogin_ReturnsUnauthorized()
        {
            // Arrange
            UsersController controller = new UsersController();

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            IActionResult result = controller.GetMyProfile();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void UpdateMyProfile_WithoutLogin_ReturnsUnauthorized()
        {
            // Arrange
            UsersController controller = new UsersController();

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            IActionResult result = controller.UpdateMyProfile(
                "Test User",
                "test@example.com");

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }

    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> values =
            new Dictionary<string, byte[]>();

        public TestSession(int userID = 0, string role = "")
        {
            if (userID != 0)
            {
                Set("userID", System.BitConverter.GetBytes(userID));
                Set("userRole", Encoding.UTF8.GetBytes(role));
            }
        }

        public bool IsAvailable => true;
        public string Id => "test-session";
        public IEnumerable<string> Keys => values.Keys;

        public void Clear() => values.Clear();

        public Task CommitAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task LoadAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public bool TryGetValue(string key, out byte[] value)
            => values.TryGetValue(key, out value!);

        public void Set(string key, byte[] value)
            => values[key] = value;

        public void Remove(string key)
            => values.Remove(key);
    }
}
