
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Racetest;
using Xunit;

namespace Racetest
{
    public class ResultsManagementTests
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
        public void UpdateResult_AsParticipant_ReturnsForbid()
        {
            var controller = CreateController(101, "Participant");

            var result = controller.UpdateResult(
                900, new System.TimeSpan(0, 30, 0), 1);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void DeleteResult_AsParticipant_ReturnsForbid()
        {
            var controller = CreateController(101, "Participant");

            var result = controller.DeleteResult(900);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void UpdateResult_InvalidID_ReturnsBadRequest()
        {
            var controller = CreateController(100, "Organiser");

            var result = controller.UpdateResult(
                0, new System.TimeSpan(0, 30, 0), 0);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void DeleteResult_InvalidID_ReturnsBadRequest()
        {
            var controller = CreateController(100, "Organiser");

            var result = controller.DeleteResult(0);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
