
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Xunit;

namespace Racetest
{
    public class UserProfileUpdateTests
    {
        private UsersController CreateController(int? userID)
        {
            var controller = new UsersController();

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            if (userID.HasValue)
            {
                httpContext.Session.SetInt32("userID", userID.Value);
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public void UpdateMyProfile_WhenNotLoggedIn_ReturnsUnauthorized()
        {
            var controller = CreateController(null);

            var result = controller.UpdateMyProfile(
                "Test User", "test@example.com");

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void UpdateMyProfile_WhenNameIsEmpty_ReturnsBadRequest()
        {
            var controller = CreateController(100);

            var result = controller.UpdateMyProfile(
                "", "test@example.com");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void UpdateMyProfile_WhenEmailIsEmpty_ReturnsBadRequest()
        {
            var controller = CreateController(100);

            var result = controller.UpdateMyProfile(
                "Test User", "");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void UpdateMyProfile_WhenNameIsTooLong_ReturnsBadRequest()
        {
            var controller = CreateController(100);
            string fullName = new string('A', 101);

            var result = controller.UpdateMyProfile(
                fullName, "test@example.com");

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}

