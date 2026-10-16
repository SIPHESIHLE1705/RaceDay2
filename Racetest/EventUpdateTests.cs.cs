
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Racetest;
using System;
using Xunit;

namespace Racetest
{
    public class EventUpdateTests
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
        public void UpdateEvent_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.UpdateEvent(
                300, "Fun Run", "Running event",
                "Johannesburg", 5,
                DateTime.Today.AddDays(7), "Run");

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void UpdateEvent_AsParticipant_ReturnsForbid()
        {
            var controller = CreateController(101, "Participant");

            var result = controller.UpdateEvent(
                300, "Fun Run", "Running event",
                "Johannesburg", 5,
                DateTime.Today.AddDays(7), "Run");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void UpdateEvent_InvalidDistance_ReturnsBadRequest()
        {
            var controller = CreateController(100, "Organiser");

            var result = controller.UpdateEvent(
                300, "Fun Run", "Running event",
                "Johannesburg", 0,
                DateTime.Today.AddDays(7), "Run");

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
