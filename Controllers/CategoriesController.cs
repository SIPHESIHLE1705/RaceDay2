using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        // Creates an instance of the database class.
        private readonly store_info db = new store_info();

        // Retrieves all categories for a specific event.
        [HttpGet]
        public IActionResult GetCategories([FromRoute] int eventID)
        {
            // Checks if the event exists.
            if (db.getEventByID(eventID) == null)
                return NotFound("Event not found.");

            // Returns the categories belonging to the event.
            return Ok(db.getCategories(eventID));
        }

        // Creates a new category for an event.
        [HttpPost]
        public IActionResult CreateCategory(
            [FromRoute] int eventID,
            [FromQuery] string categoryName,
            [FromQuery] int categoryAge,
            [FromQuery] decimal categoryDistance,
            [FromQuery] TimeSpan startTime)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0)
                return Unauthorized("Please log in first.");

            // Allows only organisers to create categories.
            if (role != "Organiser")
                return Forbid();

            // Checks if the organiser owns the event.
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            // Checks that the category name is provided.
            if (string.IsNullOrWhiteSpace(categoryName))
                return BadRequest("Category name is required.");

            // Ensures the category name does not exceed 50 characters.
            if (categoryName.Length > 50)
                return BadRequest("Category name is too long.");

            // Checks that the age is not negative.
            if (categoryAge < 0)
                return BadRequest("Age cannot be negative.");

            // Ensures the category distance is greater than zero.
            if (categoryDistance <= 0)
                return BadRequest("Distance must be greater than zero.");

            // Checks that the start time is within a valid day.
            if (startTime < TimeSpan.Zero ||
                startTime >= TimeSpan.FromDays(1))
            {
                return BadRequest("Enter a valid start time.");
            }

            // Stores the category details in a request object.
            CreateCategoryRequest request = new CreateCategoryRequest
            {
                categoryName = categoryName,
                categoryAge = categoryAge,
                categoryDistance = categoryDistance,
                startTime = startTime
            };

            // Attempts to save the category in the database.
            if (!db.createCategory(request, eventID))
                return StatusCode(500, "Could not create category.");

            // Confirms that the category was created successfully.
            return Ok("Category created successfully.");
        }

        // Updates an existing category for an event.
        [HttpPut("{categoryID:int}")]
        public IActionResult UpdateCategory(
            [FromRoute] int eventID,
            [FromRoute] int categoryID,
            [FromQuery] string categoryName,
            [FromQuery] int categoryAge,
            [FromQuery] decimal categoryDistance,
            [FromQuery] TimeSpan startTime)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0) return Unauthorized("Please log in first.");

            // Allows only organisers to update categories.
            if (role != "Organiser") return Forbid();

            // Checks if the organiser owns the event.
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            // Checks that the category name is provided.
            if (string.IsNullOrWhiteSpace(categoryName))
                return BadRequest("Category name is required.");

            // Ensures the category name does not exceed 50 characters.
            if (categoryName.Length > 50) return BadRequest("Category name is too long.");

            // Checks that the age is not negative.
            if (categoryAge < 0) return BadRequest("Age cannot be negative.");

            // Ensures the category distance is greater than zero.
            if (categoryDistance <= 0) return BadRequest("Distance must be greater than zero.");

            // Checks that the start time is within a valid day.
            if (startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1))
                return BadRequest("Enter a valid start time.");

            // Stores the updated category details in a request object.
            var request = new CreateCategoryRequest
            {
                categoryName = categoryName,
                categoryAge = categoryAge,
                categoryDistance = categoryDistance,
                startTime = startTime
            };

            // Sends the updated category details to the database.
            string result = db.updateCategory(eventID, categoryID, request);

            // Returns a success message if the category was updated.
            if (result == "SUCCESS") return Ok("Category updated successfully.");

            // Returns an error if the category does not exist.
            if (result == "NOT_FOUND") return NotFound("Category not found.");

            // Returns a conflict if a category with the same name and distance exists.
            if (result == "DUPLICATE_CATEGORY")
                return Conflict("A category with that name and distance already exists for this event.");

            // Returns an error if the category could not be updated.
            return StatusCode(500, "Could not update category.");
        }

        // Deletes a category from an event.
        [HttpDelete("{categoryID:int}")]
        public IActionResult DeleteCategory(
            [FromRoute] int eventID,
            [FromRoute] int categoryID)
        {
            // Retrieves the logged-in user's ID and role from the session.
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            // Checks if the user is logged in.
            if (userID == 0) return Unauthorized("Please log in first.");

            // Allows only organisers to delete categories.
            if (role != "Organiser") return Forbid();

            // Checks if the organiser owns the event.
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            // Attempts to delete the category from the database.
            string result = db.deleteCategory(eventID, categoryID);

            // Returns a success message if the category was deleted.
            if (result == "SUCCESS") return Ok("Category deleted successfully.");

            // Returns an error if the category does not exist.
            if (result == "NOT_FOUND") return NotFound("Category not found.");

            // Prevents deletion when the category has existing enrolments.
            if (result == "HAS_ENROLMENTS")
                return Conflict("This category has enrolments. Remove the enrolments before deleting it.");

            // Returns an error if the category could not be deleted.
            return StatusCode(500, "Could not delete category.");
        }

    }
}
