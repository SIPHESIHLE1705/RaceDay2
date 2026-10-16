using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Creates an instance of the database class.
        private readonly store_info db = new store_info();

        // Handles user login.
        [HttpPost("login")]
        public IActionResult Login(
            [FromQuery] string email,
            [FromQuery] string password)
        {
            // Checks if the email or password is empty.
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Email and password are required.");
            }

            // Retrieves the user from the database using the email.
            User? user = db.getUserByEmail(email);

            // Returns an error if the user does not exist.
            if (user == null)
                return Unauthorized("Invalid email or password.");

            bool validPassword;

            try
            {
                // Verifies the entered password against the stored password hash.
                validPassword = BCrypt.Net.BCrypt.Verify(password, user.password);
            }
            catch
            {
                // Treats the password as invalid if verification fails.
                validPassword = false;
            }

            // Returns an error if the password is incorrect.
            if (!validPassword)
                return Unauthorized("Invalid email or password.");

            // Stores the user's ID and role in the session after successful login.
            HttpContext.Session.SetInt32("userID", user.userID);
            HttpContext.Session.SetString("userRole", user.userRole);

            // Returns a success message and the user's details.
            return Ok(new
            {
                message = "Login successful.",
                userID = user.userID,
                fullName = user.fullName,
                emailAddress = user.emailAddress,
                userRole = user.userRole
            });
        }

        // Handles user logout.
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clears all session data for the current user.
            HttpContext.Session.Clear();

            // Returns a logout confirmation message.
            return Ok("Logout successful.");
        }
    }
}
