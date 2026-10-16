using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        // Creates an instance of the database class.
        private readonly store_info db = new store_info();

        // Retrieves all results from the database.
        [HttpGet]
        public IActionResult GetResults()
        {
            // Returns the list of results.
            return Ok(db.getResults());
        }

        // Creates a result for a participant's enrolment.
        [HttpPost]
        public IActionResult CreateResult(
            [FromQuery] int enrolmentID,
            [FromQuery] TimeSpan finishingTime,
            [FromQuery] int finishingPosition)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0)
                return Unauthorized("Please log in first.");

            // Allows only organisers to create results.
            if (role != "Organiser")
                return Forbid();

            // Checks that the enrolment ID and finishing position are valid.
            if (enrolmentID <= 0 || finishingPosition <= 0)
                return BadRequest("Enter valid enrolment and finishing position.");

            // Checks that the finishing time is within a valid day.
            if (finishingTime < TimeSpan.Zero ||
                finishingTime >= TimeSpan.FromDays(1))
            {
                return BadRequest("Enter a valid finishing time.");
            }

            // Retrieves the event ID associated with the enrolment.
            int? eventID = db.getEventIDForEnrolment(enrolmentID);

            // Returns an error if the enrolment does not exist.
            if (eventID == null)
                return NotFound("Enrolment not found.");

            // Checks if the organiser owns the event.
            if (!db.organiserOwnsEvent(eventID.Value, userID))
                return Forbid();

            // Stores the result details in a request object.
            CreateResultRequest request = new CreateResultRequest
            {
                enrolmentID = enrolmentID,
                finishingTime = finishingTime,
                finishingPosition = finishingPosition
            };

            // Sends the result details to the database.
            string result = db.createResult(request);

            // Prevents creating another result for the same enrolment.
            if (result == "RESULT_EXISTS")
                return Conflict("A result already exists for this enrolment.");

            // Confirms that the result was created successfully.
            if (result == "SUCCESS")
                return Ok("Result created successfully.");

            // Returns an error if the result could not be created.
            return StatusCode(500, "Could not create result.");
        }

        // Updates an existing result.
        [HttpPut("{resultID:int}")]
        public IActionResult UpdateResult(
            [FromRoute] int resultID,
            [FromQuery] TimeSpan finishingTime,
            [FromQuery] int finishingPosition)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0) return Unauthorized("Please log in first.");

            // Allows only organisers to update results.
            if (role != "Organiser") return Forbid();

            // Checks that the result ID and finishing position are valid.
            if (resultID <= 0 || finishingPosition <= 0)
                return BadRequest("Enter a valid result ID and finishing position.");

            // Checks that the finishing time is within a valid day.
            if (finishingTime < TimeSpan.Zero || finishingTime >= TimeSpan.FromDays(1))
                return BadRequest("Enter a valid finishing time.");

            // Retrieves the event ID associated with the result.
            int? eventID = db.getEventIDForResult(resultID);

            // Returns an error if the result does not exist.
            if (eventID == null) return NotFound("Result not found.");

            // Checks if the organiser owns the event.
            if (!db.organiserOwnsEvent(eventID.Value, userID)) return Forbid();

            // Sends the updated finishing time and position to the database.
            string result = db.updateResult(resultID, finishingTime, finishingPosition);

            // Confirms that the result was updated successfully.
            if (result == "SUCCESS") return Ok("Result updated successfully.");

            // Returns an error if the result does not exist.
            if (result == "NOT_FOUND") return NotFound("Result not found.");

            // Returns an error if the result could not be updated.
            return StatusCode(500, "Could not update result.");
        }

        // Deletes an existing result.
        [HttpDelete("{resultID:int}")]
        public IActionResult DeleteResult([FromRoute] int resultID)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0) return Unauthorized("Please log in first.");

            // Allows only organisers to delete results.
            if (role != "Organiser") return Forbid();

            // Checks that the result ID is valid.
            if (resultID <= 0) return BadRequest("Enter a valid result ID.");

            // Retrieves the event ID associated with the result.
            int? eventID = db.getEventIDForResult(resultID);

            // Returns an error if the result does not exist.
            if (eventID == null) return NotFound("Result not found.");

            // Checks if the organiser owns the event.
            if (!db.organiserOwnsEvent(eventID.Value, userID)) return Forbid();

            // Attempts to delete the result from the database.
            string result = db.deleteResult(resultID);

            // Confirms that the result was deleted successfully.
            if (result == "SUCCESS") return Ok("Result deleted successfully.");

            // Returns an error if the result does not exist.
            if (result == "NOT_FOUND") return NotFound("Result not found.");

            // Returns an error if the result could not be deleted.
            return StatusCode(500, "Could not delete result.");
        }

    }
}
