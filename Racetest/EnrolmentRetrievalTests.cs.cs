
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Racetest;
using Xunit;

namespace Racetest
{
    public class EnrolmentRetrievalTests
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
        public void GetMyEnrolments_WithoutLogin_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = controller.GetMyEnrolments();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void GetMyEnrolments_AsOrganiser_ReturnsForbid()
        {
            var controller = CreateController(100, "Organiser");

            var result = controller.GetMyEnrolments();

            Assert.IsType<ForbidResult>(result);
        }
    }
}
