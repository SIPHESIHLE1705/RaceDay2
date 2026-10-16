using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // Creates an instance of the database class.
        private readonly store_info db = new store_info();

        // Registers a new user.
        [HttpPost("register")]
        public IActionResult Register(
            [FromQuery] string fullname,
            [FromQuery] string email,
            [FromQuery] string password,
            [FromQuery] string role)
        {
            // Checks that all registration fields are provided.
            if (string.IsNullOrWhiteSpace(fullname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(role))
            {
                return BadRequest("All fields are required.");
            }

            // Ensures the password contains at least 8 characters.
            if (password.Length < 8)
                return BadRequest("Password must contain at least 8 characters.");

            // Checks that the name and email do not exceed 100 characters.
            if (fullname.Length > 100 || email.Length > 100)
                return BadRequest("Name or email is too long.");

            // Allows users to register only as participants or organisers.
            if (role != "Participant" && role != "Organiser")
                return BadRequest("Role must be Participant or Organiser.");

            // Sends the new user's details to the database.
            string result = db.createUser(fullname, email, role, password);

            // Returns an error if the email is already registered.
            if (result == "EMAIL_EXISTS")
                return Conflict("Email already registered.");

            // Confirms that registration was successful.
            if (result == "SUCCESS")
                return Ok("Registration successful.");

            // Returns an error if registration fails.
            return StatusCode(500, "Registration failed.");
        }

        // Retrieves the profile of the logged-in user.
        [HttpGet("me")]
        public IActionResult GetMyProfile()
        {
            // Retrieves the logged-in user's ID from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;

            // Checks if the user is logged in.
            if (userID == 0)
                return Unauthorized("Please log in first.");

            // Retrieves the user's details from the database.
            User? user = db.getUserByID(userID);

            // Returns an error if the user does not exist.
            if (user == null)
                return NotFound("User not found.");

            // Returns the user's profile details.
            return Ok(user);
        }

        // Updates the profile of the logged-in user.
        [HttpPut("me")]
        public IActionResult UpdateMyProfile(
            [FromQuery] string fullName,
            [FromQuery] string emailAddress)
        {
            // Retrieves the logged-in user's ID from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;

            // Checks if the user is logged in.
            if (userID == 0)
                return Unauthorized("Please log in first.");

            // Checks that the name and email are provided.
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(emailAddress))
                return BadRequest("Name and email are required.");

            // Checks that the name and email do not exceed 100 characters.
            if (fullName.Length > 100 || emailAddress.Length > 100)
                return BadRequest("Name or email is too long.");

            // Sends the updated profile details to the database.
            string result = db.updateUser(userID, fullName, emailAddress);

            // Returns an error if the email is already registered.
            if (result == "EMAIL_EXISTS")
                return Conflict("Email already registered.");

            // Returns an error if the user does not exist.
            if (result == "NOT_FOUND")
                return NotFound("User not found.");

            // Confirms that the profile was updated successfully.
            if (result == "SUCCESS")
                return Ok("Profile updated successfully.");

            // Returns an error if the profile could not be updated.
            return StatusCode(500, "Could not update profile.");
        }

    }
}
