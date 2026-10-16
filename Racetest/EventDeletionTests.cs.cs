
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Racetest;
using Xunit;

namespace Racetest
{
    public class EventDeletionTests
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
        public void DeleteEvent_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.DeleteEvent(300);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void DeleteEvent_AsParticipant_ReturnsForbid()
        {
            var controller = CreateController(101, "Participant");

            var result = controller.DeleteEvent(300);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void DeleteEvent_InvalidID_ReturnsBadRequest()
        {
            var controller = CreateController(100, "Organiser");

            var result = controller.DeleteEvent(0);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
