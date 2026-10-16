using Microsoft.AspNetCore.Mvc;
using Race.Controllers;

namespace Racetest
{
    
    public class RegistrationTests
    {
        [Fact]
        public void Register_EmptyFields_ReturnsBadRequest()
        {
            // Arrange
            UsersController controller = new UsersController();

            // Act
            IActionResult result = controller.Register(
                "", "", "", "");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Register_ShortPassword_ReturnsBadRequest()
        {
            // Arrange
            UsersController controller = new UsersController();

            // Act
            IActionResult result = controller.Register(
                "Test Participant",
                "participant@example.com",
                "123",
                "Participant");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Register_InvalidRole_ReturnsBadRequest()
        {
            // Arrange
            UsersController controller = new UsersController();

            // Act
            IActionResult result = controller.Register(
                "Test User",
                "test@example.com",
                "password123",
                "Admin");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Register_ValidInput_DoesNotReturnBadRequest()
        {
            // Arrange
            UsersController controller = new UsersController();

            // Act
            IActionResult result = controller.Register(
                "Test Participant",
                "participant@example.com",
                "password123",
                "Participant");

            // Assert
            Assert.IsNotType<BadRequestObjectResult>(result);
        }
    }
}

