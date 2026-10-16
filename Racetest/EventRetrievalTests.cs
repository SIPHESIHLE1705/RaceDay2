
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Xunit;

namespace Racetest
{
    public class EventRetrievalTests
    {
        [Fact]
        public void GetEvent_InvalidID_ReturnsNotFound()
        {
            // Arrange
            var controller = new EventsController();

            // Act
            var result = controller.GetEvent(0);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
