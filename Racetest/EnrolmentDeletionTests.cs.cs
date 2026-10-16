
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Racetest;
using Xunit;

namespace Racetest
{
    public class EnrolmentDeletionTests
    {
        private EnrolmentsController CreateController(
            int userID = 0,
            string role = "")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(userID, role);

            return new EnrolmentsController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public void DeleteEnrolment_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.DeleteEnrolment(700);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void DeleteEnrolment_AsOrganiser_ReturnsForbid()
        {
            var controller = CreateController(100, "Organiser");

            var result = controller.DeleteEnrolment(700);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void DeleteEnrolment_InvalidID_ReturnsBadRequest()
        {
            var controller = CreateController(101, "Participant");

            var result = controller.DeleteEnrolment(0);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
