using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        // Creates an instance of the database class.
        private readonly store_info db = new store_info();

        // Creates an enrolment for a participant.
        [HttpPost]
        public IActionResult CreateEnrolment(
            [FromQuery] int eventID,
            [FromQuery] int categoryID)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0)
                return Unauthorized("Please log in first.");

            // Allows only participants to enrol in events.
            if (role != "Participant")
                return Forbid();

            // Checks that the event ID and category ID are valid.
            if (eventID <= 0 || categoryID <= 0)
                return BadRequest("Valid event and category IDs are required.");

            // Sends the enrolment details to the database.
            string result = db.createEnrolment(
                userID, eventID, categoryID);

            // Returns an error if the category does not belong to the event.
            if (result == "CATEGORY_NOT_FOUND")
                return BadRequest("Category does not belong to this event.");

            // Prevents a participant from enrolling in the same event again.
            if (result == "ALREADY_ENROLLED")
                return Conflict("You are already enrolled in this event.");

            // Confirms that the enrolment was successful.
            if (result == "SUCCESS")
                return Ok("Enrolment successful.");

            // Returns an error if the enrolment fails.
            return StatusCode(500, "Enrolment failed.");
        }

        // Retrieves all enrolments belonging to the logged-in participant.
        [HttpGet("my")]
        public IActionResult GetMyEnrolments()
        {
            // Retrieves the logged-in user's ID from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;

            // Checks if the user is logged in.
            if (userID == 0)
                return Unauthorized("Please log in first.");

            // Allows only participants to view their enrolments.
            if (HttpContext.Session.GetString("userRole") != "Participant")
                return Forbid();

            // Retrieves and returns the participant's enrolments.
            return Ok(db.getEnrolments(userID));
        }

        // Deletes an enrolment belonging to the logged-in participant.
        [HttpDelete("{enrolmentID:int}")]
        public IActionResult DeleteEnrolment([FromRoute] int enrolmentID)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0) return Unauthorized("Please log in first.");

            // Allows only participants to delete enrolments.
            if (role != "Participant") return Forbid();

            // Checks that the enrolment ID is valid.
            if (enrolmentID <= 0) return BadRequest("Enter a valid enrolment ID.");

            // Attempts to delete the enrolment for the logged-in participant.
            string result = db.deleteEnrolment(enrolmentID, userID);

            // Confirms that the enrolment was deleted successfully.
            if (result == "SUCCESS") return Ok("Enrolment deleted successfully.");

            // Returns a conflict if the enrolment does not exist, belongs to another user,
            // or already has a result recorded.
            if (result == "NOT_FOUND_OR_HAS_RESULT")
                return Conflict("Enrolment was not found, does not belong to you, or already has a result.");

            // Returns an error if the enrolment could not be deleted.
            return StatusCode(500, "Could not delete enrolment.");
        }

    }
}
