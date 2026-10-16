
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Racetest;
using Xunit;

namespace Racetest
{
    public class CategoryManagementTests
    {
        private CategoriesController CreateController(
            int userID = 0,
            string role = "")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(userID, role);

            return new CategoriesController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public void UpdateCategory_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.UpdateCategory(
                300, 500, "Adults", 18, 5,
                new System.TimeSpan(8, 0, 0));

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void UpdateCategory_AsParticipant_ReturnsForbid()
        {
            var controller = CreateController(101, "Participant");

            var result = controller.UpdateCategory(
                300, 500, "Adults", 18, 5,
                new System.TimeSpan(8, 0, 0));

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void DeleteCategory_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.DeleteCategory(300, 500);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void DeleteCategory_AsParticipant_ReturnsForbid()
        {
            var controller = CreateController(101, "Participant");

            var result = controller.DeleteCategory(300, 500);

            Assert.IsType<ForbidResult>(result);
        }
    }
}
