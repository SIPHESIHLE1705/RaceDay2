using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        // Creates an instance of the database class.
        private readonly store_info db = new store_info();

        // Retrieves all events from the database.
        [HttpGet]
        public IActionResult GetEvents()
        {
            // Returns the list of events.
            return Ok(db.getEvents());
        }

        // Retrieves a specific event using its ID.
        [HttpGet("{eventID:int}")]
        public IActionResult GetEvent([FromRoute] int eventID)
        {
            // Searches for the event in the database.
            Event? item = db.getEventByID(eventID);

            // Returns an error if the event does not exist.
            if (item == null)
                return NotFound("Event not found.");

            // Returns the event details.
            return Ok(item);
        }

        // Creates a new event.
        [HttpPost]
        public IActionResult CreateEvent(
            [FromQuery] string eventName,
            [FromQuery] string eventDescription,
            [FromQuery] string eventLocation,
            [FromQuery] decimal eventDistance,
            [FromQuery] DateTime eventDate,
            [FromQuery] string eventType)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0)
                return Unauthorized("Please log in first.");

            // Allows only organisers to create events.
            if (role != "Organiser")
                return Forbid();

            // Checks that all required event fields are provided.
            if (string.IsNullOrWhiteSpace(eventName) ||
                string.IsNullOrWhiteSpace(eventDescription) ||
                string.IsNullOrWhiteSpace(eventLocation))
            {
                return BadRequest("All event fields are required.");
            }

            // Checks that the event fields do not exceed their character limits.
            if (eventName.Length > 100 ||
                eventDescription.Length > 500 ||
                eventLocation.Length > 100)
            {
                return BadRequest("One or more fields are too long.");
            }

            // Ensures the event distance is greater than zero.
            if (eventDistance <= 0)
                return BadRequest("Distance must be greater than zero.");

            // Checks that a valid event date has been provided.
            if (eventDate == default)
                return BadRequest("A valid event date is required.");

            // Ensures the event type is Run, Walk or Cycle.
            if (eventType != "Run" &&
                eventType != "Walk" &&
                eventType != "Cycle")
            {
                return BadRequest("Event type must be Run, Walk or Cycle.");
            }

            // Stores the event details in a request object.
            CreateEventRequest request = new CreateEventRequest
            {
                eventName = eventName,
                eventDescription = eventDescription,
                eventLocation = eventLocation,
                eventDistance = eventDistance,
                eventDate = eventDate,
                eventType = eventType
            };

            // Attempts to save the event in the database.
            if (!db.createEvent(request, userID))
                return StatusCode(500, "Could not create event.");

            // Confirms that the event was created successfully.
            return Ok("Event created successfully.");
        }

        // Updates an existing event.
        [HttpPut("{eventID:int}")]
        public IActionResult UpdateEvent(
            [FromRoute] int eventID,
            [FromQuery] string eventName,
            [FromQuery] string eventDescription,
            [FromQuery] string eventLocation,
            [FromQuery] decimal eventDistance,
            [FromQuery] DateTime eventDate,
            [FromQuery] string eventType)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0) return Unauthorized("Please log in first.");

            // Allows only organisers to update events.
            if (role != "Organiser") return Forbid();

            // Checks if the organiser owns the event.
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            // Checks that all required event fields are provided.
            if (string.IsNullOrWhiteSpace(eventName) ||
                string.IsNullOrWhiteSpace(eventDescription) ||
                string.IsNullOrWhiteSpace(eventLocation))
                return BadRequest("All event fields are required.");

            // Checks that the event fields do not exceed their character limits.
            if (eventName.Length > 100 || eventDescription.Length > 500 ||
                eventLocation.Length > 100)
                return BadRequest("One or more fields are too long.");

            // Ensures the event distance is greater than zero.
            if (eventDistance <= 0) return BadRequest("Distance must be greater than zero.");

            // Checks that a valid event date has been provided.
            if (eventDate == default) return BadRequest("A valid event date is required.");

            // Ensures the event type is Run, Walk or Cycle.
            if (eventType != "Run" && eventType != "Walk" && eventType != "Cycle")
                return BadRequest("Event type must be Run, Walk or Cycle.");

            // Stores the updated event details in a request object.
            var request = new CreateEventRequest
            {
                eventName = eventName,
                eventDescription = eventDescription,
                eventLocation = eventLocation,
                eventDistance = eventDistance,
                eventDate = eventDate,
                eventType = eventType
            };

            // Sends the updated event details to the database.
            string result = db.updateEvent(eventID, userID, request);

            // Confirms that the event was updated successfully.
            if (result == "SUCCESS") return Ok("Event updated successfully.");

            // Returns an error if the event does not exist.
            if (result == "NOT_FOUND") return NotFound("Event not found.");

            // Returns an error if the event could not be updated.
            return StatusCode(500, "Could not update event.");
        }

        // Deletes an existing event.
        [HttpDelete("{eventID:int}")]
        public IActionResult DeleteEvent([FromRoute] int eventID)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0) return Unauthorized("Please log in first.");

            // Allows only organisers to delete events.
            if (role != "Organiser") return Forbid();

            // Checks if the organiser owns the event.
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            // Attempts to delete the event from the database.
            string result = db.deleteEvent(eventID, userID);

            // Confirms that the event was deleted successfully.
            if (result == "SUCCESS") return Ok("Event deleted successfully.");

            // Prevents deletion when dependent records still exist.
            if (result == "HAS_DEPENDENCIES")
                return Conflict("Delete dependent results, enrolments and categories before deleting the event.");

            // Returns an error if the event does not exist.
            if (result == "NOT_FOUND") return NotFound("Event not found.");

            // Returns an error if the event could not be deleted.
            return StatusCode(500, "Could not delete event.");
        }

    }
}
