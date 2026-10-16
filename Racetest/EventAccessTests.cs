
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Racetest;
using Xunit;

namespace Racetest
{
    public class EventAccessTests
    {
        private EventsController CreateController(
            int userID = 0,
            string role = "")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(userID, role);

            return new EventsController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public void CreateEvent_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.CreateEvent(
                "Fun Run",
                "A running event",
                "Johannesburg",
                5,
                System.DateTime.Today.AddDays(7),
                "Run");

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void CreateEvent_AsParticipant_ReturnsForbid()
        {
            var controller = CreateController(100, "Participant");

            var result = controller.CreateEvent(
                "Fun Run",
                "A running event",
                "Johannesburg",
                5,
                System.DateTime.Today.AddDays(7),
                "Run");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void CreateEvent_InvalidDistance_ReturnsBadRequest()
        {
            var controller = CreateController(101, "Organiser");

            var result = controller.CreateEvent(
                "Fun Run",
                "A running event",
                "Johannesburg",
                0,
                System.DateTime.Today.AddDays(7),
                "Run");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateEvent_InvalidEventType_ReturnsBadRequest()
        {
            var controller = CreateController(101, "Organiser");

            var result = controller.CreateEvent(
                "Fun Run",
                "A running event",
                "Johannesburg",
                5,
                System.DateTime.Today.AddDays(7),
                "Swimming");

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
