
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Racetest;
using Xunit;

namespace Racetest
{
    public class ResultsAccessTests
    {
        private ResultsController CreateController(
            int userID = 0,
            string role = "")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(userID, role);

            return new ResultsController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public void CreateResult_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.CreateResult(700,
                new System.TimeSpan(0, 30, 0), 1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void CreateResult_AsParticipant_ReturnsForbid()
        {
            var controller = CreateController(101, "Participant");

            var result = controller.CreateResult(700,
                new System.TimeSpan(0, 30, 0), 1);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void CreateResult_InvalidIDs_ReturnsBadRequest()
        {
            var controller = CreateController(100, "Organiser");

            var result = controller.CreateResult(0,
                new System.TimeSpan(0, 30, 0), 0);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void UpdateResult_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.UpdateResult(900,
                new System.TimeSpan(0, 30, 0), 1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void DeleteResult_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.DeleteResult(900);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
