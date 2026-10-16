using Race.Models;
using System.Data;
using System.Data.SqlClient;

namespace Race.Database
{
    public class store_info
    {
        // Connection string used to connect to the RaceDay database.
        public string connection_string =
            @"Data source =(localdb)\api_both;database=RaceDay";

        // Stores database error messages when an SQL exception occurs.
        public string message = string.Empty;

        // REGISTER USER
        // Checks whether the email already exists, hashes the password,
        // and saves the new user's details in the database.
        public string createUser(
            string fullname, string email,
            string role, string password)
        {
            try
            {
                // Creates and manages the database connection.
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    con.Open();

                    // Checks whether another user already uses this email.
                    string check = @"SELECT COUNT(*)
                                     FROM Users
                                     WHERE emailAddress = @email";

                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        // Adds the email as a parameter to the SQL query.
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        // Stops registration if the email already exists.
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            return "EMAIL_EXISTS";
                    }

                    // Hashes the password before storing it in the database.
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                    // SQL query to insert the new user's details.
                    string query = @"INSERT INTO Users
                        (fullName, emailAddress, [password], userRole)
                        VALUES (@name, @email, @password, @role)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the user's full name.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100)
                            .Value = fullname;

                        // Supplies the user's email address.
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        // Stores the hashed password instead of the plain password.
                        cmd.Parameters.Add("@password", SqlDbType.VarChar, 255)
                            .Value = hashedPassword;

                        // Supplies the user's role, such as Participant or Organiser.
                        cmd.Parameters.Add("@role", SqlDbType.VarChar, 20)
                            .Value = role;

                        // Returns SUCCESS if a row was inserted; otherwise, FAILED.
                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error and returns a database error status.
                message = ex.Message;
                return "DATABASE_ERROR";
            }
        }

        // GET USER BY EMAIL
        // Finds a user by email and returns their details as a User object.
        public User? getUserByEmail(string email)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects the user details needed for authentication.
                    string query = @"SELECT userID, fullName,
                        emailAddress, [password], userRole
                        FROM Users
                        WHERE emailAddress = @email";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the email used to find the account.
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        con.Open();

                        // Reads the matching database record.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Converts the database values into a User object.
                                return new User
                                {
                                    userID = Convert.ToInt32(reader["userID"]),
                                    fullName = reader["fullName"].ToString()!,
                                    emailAddress = reader["emailAddress"].ToString()!,
                                    password = reader["password"].ToString()!,
                                    userRole = reader["userRole"].ToString()!
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error if the lookup fails.
                message = ex.Message;
            }

            // Returns null when no matching user is found or an SQL error occurs.
            return null;
        }

        // GET USER BY ID
        // Retrieves a user's profile details using their user ID.
        public User? getUserByID(int userID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects the user whose ID matches the supplied value.
                    string query = @"SELECT userID, fullName,
                        emailAddress, userRole
                        FROM Users
                        WHERE userID = @userID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the ID used to find the user.
                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        con.Open();

                        // Reads the matching user record.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Creates a User object using the retrieved values.
                                return new User
                                {
                                    userID = Convert.ToInt32(reader["userID"]),
                                    fullName = reader["fullName"].ToString()!,
                                    emailAddress = reader["emailAddress"].ToString()!,
                                    userRole = reader["userRole"].ToString()!
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error if the lookup fails.
                message = ex.Message;
            }

            // Returns null if the user cannot be found.
            return null;
        }

        // CREATE EVENT
        // Saves a new event and links it to the organiser creating it.
        public bool createEvent(
            CreateEventRequest request, int organiserID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // SQL query to insert the event details.
                    string query = @"INSERT INTO Events
                        (eventName, eventDescription, eventLocation,
                         eventDistance, eventDate, eventType, organiserID)
                        VALUES
                        (@name, @description, @location, @distance,
                         @date, @type, @organiserID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Adds the event name.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100)
                            .Value = request.eventName;

                        // Adds the event description.
                        cmd.Parameters.Add("@description", SqlDbType.VarChar, 500)
                            .Value = request.eventDescription;

                        // Adds the event location.
                        cmd.Parameters.Add("@location", SqlDbType.VarChar, 100)
                            .Value = request.eventLocation;

                        // Defines the decimal precision for the event distance.
                        SqlParameter distance =
                            cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8;
                        distance.Scale = 2;
                        distance.Value = request.eventDistance;

                        // Stores the event date without its time component.
                        cmd.Parameters.Add("@date", SqlDbType.Date)
                            .Value = request.eventDate.Date;

                        // Adds the event type.
                        cmd.Parameters.Add("@type", SqlDbType.VarChar, 20)
                            .Value = request.eventType;

                        // Links the event to the organiser's user ID.
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int)
                            .Value = organiserID;

                        con.Open();

                        // Returns true if the event was inserted successfully.
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error and indicates that creation failed.
                message = ex.Message;
                return false;
            }
        }

        // GET ALL EVENTS
        // Retrieves all events and returns them as a list.
        public List<Event> getEvents()
        {
            // Creates an empty list to store the retrieved events.
            List<Event> events = new List<Event>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Retrieves event details and sorts them by date.
                    string query = @"SELECT eventID, eventName,
                        eventDescription, eventLocation, eventDistance,
                        eventDate, eventType, organiserID
                        FROM Events
                        ORDER BY eventDate";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();

                        // Reads each event returned by the query.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Converts each database row into an Event object.
                                events.Add(new Event
                                {
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    eventName = reader["eventName"].ToString()!,
                                    eventDescription = reader["eventDescription"].ToString()!,
                                    eventLocation = reader["eventLocation"].ToString()!,
                                    eventDistance = Convert.ToDecimal(reader["eventDistance"]),
                                    eventDate = Convert.ToDateTime(reader["eventDate"]),
                                    eventType = reader["eventType"].ToString()!,
                                    organiserID = Convert.ToInt32(reader["organiserID"])
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error if retrieving events fails.
                message = ex.Message;
            }

            // Returns the list, including an empty list if no events are found.
            return events;
        }

        // GET EVENT BY ID
        // Retrieves one event using its event ID.
        public Event? getEventByID(int eventID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects the event matching the supplied ID.
                    string query = @"SELECT eventID, eventName,
                        eventDescription, eventLocation, eventDistance,
                        eventDate, eventType, organiserID
                        FROM Events
                        WHERE eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the event ID to the query.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        con.Open();

                        // Reads the matching event, if one exists.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Creates an Event object from the database row.
                                return new Event
                                {
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    eventName = reader["eventName"].ToString()!,
                                    eventDescription = reader["eventDescription"].ToString()!,
                                    eventLocation = reader["eventLocation"].ToString()!,
                                    eventDistance = Convert.ToDecimal(reader["eventDistance"]),
                                    eventDate = Convert.ToDateTime(reader["eventDate"]),
                                    eventType = reader["eventType"].ToString()!,
                                    organiserID = Convert.ToInt32(reader["organiserID"])
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error if the query fails.
                message = ex.Message;
            }

            // Returns null when the event is not found.
            return null;
        }

        // CHECK EVENT OWNERSHIP
        // Checks whether the specified organiser owns the specified event.
        public bool organiserOwnsEvent(int eventID, int organiserID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Counts records matching both the event and organiser IDs.
                    string query = @"SELECT COUNT(*)
                        FROM Events
                        WHERE eventID = @eventID
                        AND organiserID = @organiserID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the event ID.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        // Supplies the organiser ID.
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int)
                            .Value = organiserID;

                        con.Open();

                        // Returns true if a matching event belongs to the organiser.
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error and indicates that ownership was not confirmed.
                message = ex.Message;
                return false;
            }
        }

        // CREATE CATEGORY
        // Adds a category to the specified event.
        public bool createCategory(
            CreateCategoryRequest request, int eventID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Inserts the category details and associated event ID.
                    string query = @"INSERT INTO Categories
                        (categoryName, categoryAge, categoryDistance,
                         startTime, eventID)
                        VALUES
                        (@name, @age, @distance, @startTime, @eventID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Adds the category name.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 50)
                            .Value = request.categoryName;

                        // Adds the category age requirement.
                        cmd.Parameters.Add("@age", SqlDbType.Int)
                            .Value = request.categoryAge;

                        // Defines the decimal precision for the category distance.
                        SqlParameter distance =
                            cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8;
                        distance.Scale = 2;
                        distance.Value = request.categoryDistance;

                        // Adds the category's start time.
                        cmd.Parameters.Add("@startTime", SqlDbType.Time)
                            .Value = request.startTime;

                        // Links the category to its event.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        con.Open();

                        // Returns true if the category was inserted.
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error and indicates creation failed.
                message = ex.Message;
                return false;
            }
        }

        // GET CATEGORIES BY EVENT
        // Retrieves all categories associated with a particular event.
        public List<Category> getCategories(int eventID)
        {
            // Creates an empty list to hold the categories.
            List<Category> categories = new List<Category>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects categories belonging to the specified event.
                    string query = @"SELECT categoryID, categoryName,
                        categoryAge, categoryDistance, startTime, eventID
                        FROM Categories
                        WHERE eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the event ID.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        con.Open();

                        // Reads each category returned by the query.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Converts the database row into a Category object.
                                categories.Add(new Category
                                {
                                    categoryID = Convert.ToInt32(reader["categoryID"]),
                                    categoryName = reader["categoryName"].ToString()!,
                                    categoryAge = Convert.ToInt32(reader["categoryAge"]),
                                    categoryDistance = Convert.ToDecimal(reader["categoryDistance"]),
                                    startTime = (TimeSpan)reader["startTime"],
                                    eventID = Convert.ToInt32(reader["eventID"])
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error if category retrieval fails.
                message = ex.Message;
            }

            // Returns the categories found, or an empty list.
            return categories;
        }

        // CREATE ENROLMENT
        // Checks that the category belongs to the event before enrolling a user.
        public string createEnrolment(
            int userID, int eventID, int categoryID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    con.Open();

                    // Checks whether the selected category belongs to this event.
                    string check = @"SELECT COUNT(*)
                        FROM Categories
                        WHERE categoryID = @categoryID
                        AND eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        // Supplies the category ID.
                        cmd.Parameters.Add("@categoryID", SqlDbType.Int)
                            .Value = categoryID;

                        // Supplies the event ID.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        // Stops the enrolment if the category does not exist for this event.
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                            return "CATEGORY_NOT_FOUND";
                    }

                    // Inserts the enrolment with today's date and the selected IDs.
                    string query = @"INSERT INTO Enrolments
                        (enrolmentDate, userID, eventID, categoryID)
                        VALUES (@date, @userID, @eventID, @categoryID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Records the current date as the enrolment date.
                        cmd.Parameters.Add("@date", SqlDbType.Date)
                            .Value = DateTime.Today;

                        // Supplies the participant's user ID.
                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        // Supplies the event ID.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        // Supplies the selected category ID.
                        cmd.Parameters.Add("@categoryID", SqlDbType.Int)
                            .Value = categoryID;

                        // Returns SUCCESS if the enrolment was inserted.
                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error.
                message = ex.Message;

                // SQL Server error 2601 or 2627 indicates a duplicate-key violation.
                if (ex.Number == 2601 || ex.Number == 2627)
                    return "ALREADY_ENROLLED";

                // Returns a general database error for other SQL exceptions.
                return "DATABASE_ERROR";
            }
        }

        // GET PARTICIPANT ENROLMENTS
        // Retrieves enrolments belonging to the specified participant.
        public List<Enrolment> getEnrolments(int userID)
        {
            // Creates an empty list to store the participant's enrolments.
            List<Enrolment> enrolments = new List<Enrolment>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Retrieves the participant's enrolments, newest first.
                    string query = @"SELECT enrolmentID, enrolmentDate,
                        userID, eventID, categoryID
                        FROM Enrolments
                        WHERE userID = @userID
                        ORDER BY enrolmentDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the participant's user ID.
                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        con.Open();

                        // Reads each enrolment returned by the query.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Converts the database row into an Enrolment object.
                                enrolments.Add(new Enrolment
                                {
                                    enrolmentID = Convert.ToInt32(reader["enrolmentID"]),
                                    enrolmentDate = Convert.ToDateTime(reader["enrolmentDate"]),
                                    userID = Convert.ToInt32(reader["userID"]),
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    categoryID = Convert.ToInt32(reader["categoryID"])
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error if enrolments cannot be retrieved.
                message = ex.Message;
            }

            // Returns the participant's enrolments or an empty list.
            return enrolments;
        }

        // GET EVENT ID FOR ENROLMENT
        // Finds the event associated with a particular enrolment.
        public int? getEventIDForEnrolment(int enrolmentID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects the event ID linked to the enrolment.
                    string query = @"SELECT eventID
                        FROM Enrolments
                        WHERE enrolmentID = @enrolmentID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the enrolment ID.
                        cmd.Parameters.Add("@enrolmentID", SqlDbType.Int)
                            .Value = enrolmentID;

                        con.Open();

                        // Retrieves the first value returned by the query.
                        object? value = cmd.ExecuteScalar();

                        // Returns null if the enrolment or event ID is unavailable.
                        if (value == null || value == DBNull.Value)
                            return null;

                        // Converts the retrieved value into an integer event ID.
                        return Convert.ToInt32(value);
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error and returns null.
                message = ex.Message;
                return null;
            }
        }

        // CREATE RESULT
        // Saves a participant's finishing time, position and enrolment ID.
        public string createResult(CreateResultRequest request)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Inserts the result details into the Results table.
                    string query = @"INSERT INTO Results
                        (finishingTime, finishingPosition, enrolmentID)
                        VALUES (@time, @position, @enrolmentID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the participant's finishing time.
                        cmd.Parameters.Add("@time", SqlDbType.Time)
                            .Value = request.finishingTime;

                        // Supplies the participant's finishing position.
                        cmd.Parameters.Add("@position", SqlDbType.Int)
                            .Value = request.finishingPosition;

                        // Links the result to the participant's enrolment.
                        cmd.Parameters.Add("@enrolmentID", SqlDbType.Int)
                            .Value = request.enrolmentID;

                        con.Open();

                        // Returns SUCCESS if the result was inserted.
                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error.
                message = ex.Message;

                // Returns RESULT_EXISTS for a duplicate-key violation.
                if (ex.Number == 2601 || ex.Number == 2627)
                    return "RESULT_EXISTS";

                // Returns a database error for other SQL exceptions.
                return "DATABASE_ERROR";
            }
        }

        // GET ALL RESULTS
        // Retrieves all recorded results and orders them by finishing position.
        public List<Result> getResults()
        {
            // Creates an empty list to store the results.
            List<Result> results = new List<Result>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects result details, ordered by finishing position.
                    string query = @"SELECT resultID, finishingTime,
                        finishingPosition, enrolmentID
                        FROM Results
                        ORDER BY finishingPosition";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();

                        // Reads each result returned by the query.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Converts each database row into a Result object.
                                results.Add(new Result
                                {
                                    resultID = Convert.ToInt32(reader["resultID"]),
                                    finishingTime = (TimeSpan)reader["finishingTime"],
                                    finishingPosition = Convert.ToInt32(reader["finishingPosition"]),
                                    enrolmentID = Convert.ToInt32(reader["enrolmentID"])
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error if results cannot be retrieved.
                message = ex.Message;
            }

            // Returns the results list.
            return results;
        }

        // UPDATE USER PROFILE
        // Updates a user's full name and email address.
        public string updateUser(int userID, string fullName, string emailAddress)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    con.Open();

                    // Checks whether another user already uses the supplied email.
                    string check = @"SELECT COUNT(*) FROM Users
                                     WHERE emailAddress = @email AND userID <> @userID";
                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        // Supplies the new email and current user's ID.
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = emailAddress;
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

                        // Prevents the profile from being updated with another user's email.
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            return "EMAIL_EXISTS";
                    }

                    // Updates the user's name and email for the specified user ID.
                    string query = @"UPDATE Users SET fullName = @name, emailAddress = @email
                                     WHERE userID = @userID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the updated profile details.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = fullName;
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = emailAddress;
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

                        // Returns SUCCESS if a record was updated, otherwise NOT_FOUND.
                        return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                    }
                }
            }
            // Saves the SQL error and returns a database error status.
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // UPDATE EVENT (owner must be checked by controller)
        // Updates event details only when both the event ID and organiser ID match.
        public string updateEvent(int eventID, int organiserID, CreateEventRequest request)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    // Updates the specified event belonging to the organiser.
                    string query = @"UPDATE Events SET eventName=@name, eventDescription=@description,
                        eventLocation=@location, eventDistance=@distance, eventDate=@date, eventType=@type
                        WHERE eventID=@eventID AND organiserID=@organiserID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the updated event details.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = request.eventName;
                        cmd.Parameters.Add("@description", SqlDbType.VarChar, 500).Value = request.eventDescription;
                        cmd.Parameters.Add("@location", SqlDbType.VarChar, 100).Value = request.eventLocation;

                        // Sets the decimal precision and scale for the event distance.
                        SqlParameter distance = cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8; distance.Scale = 2; distance.Value = request.eventDistance;

                        // Supplies the updated date and event type.
                        cmd.Parameters.Add("@date", SqlDbType.Date).Value = request.eventDate.Date;
                        cmd.Parameters.Add("@type", SqlDbType.VarChar, 20).Value = request.eventType;

                        // Identifies the event and its organiser.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int).Value = organiserID;

                        con.Open();

                        // Returns SUCCESS if updated, otherwise NOT_FOUND.
                        return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                    }
                }
            }
            // Saves the SQL error and returns a database error status.
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // DELETE EVENT. SQL Server will reject deletion when dependent rows exist.
        // Deletes an event only if it belongs to the specified organiser.
        public string deleteEvent(int eventID, int organiserID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Events WHERE eventID=@eventID AND organiserID=@organiserID", con))
                {
                    // Supplies the event and organiser IDs for the delete operation.
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@organiserID", SqlDbType.Int).Value = organiserID;

                    con.Open();

                    // Returns SUCCESS if deleted, otherwise NOT_FOUND.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error.
                message = ex.Message;

                // Error 547 indicates a foreign-key dependency prevented deletion.
                return ex.Number == 547 ? "HAS_DEPENDENCIES" : "DATABASE_ERROR";
            }
        }

        // UPDATE CATEGORY
        // Updates category details for the specified event and category.
        public string updateCategory(int eventID, int categoryID, CreateCategoryRequest request)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                // Updates only the category matching both supplied IDs.
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Categories SET categoryName=@name,
                    categoryAge=@age, categoryDistance=@distance, startTime=@startTime
                    WHERE eventID=@eventID AND categoryID=@categoryID", con))
                {
                    // Supplies the updated category name and age.
                    cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = request.categoryName;
                    cmd.Parameters.Add("@age", SqlDbType.Int).Value = request.categoryAge;

                    // Sets the decimal precision and scale for the category distance.
                    SqlParameter distance = cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                    distance.Precision = 8; distance.Scale = 2; distance.Value = request.categoryDistance;

                    // Supplies the updated start time and identifies the category.
                    cmd.Parameters.Add("@startTime", SqlDbType.Time).Value = request.startTime;
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@categoryID", SqlDbType.Int).Value = categoryID;

                    con.Open();

                    // Returns SUCCESS if updated, otherwise NOT_FOUND.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error.
                message = ex.Message;

                // Returns DUPLICATE_CATEGORY for a duplicate-key violation.
                if (ex.Number == 2601 || ex.Number == 2627) return "DUPLICATE_CATEGORY";

                // Returns a database error for other SQL exceptions.
                return "DATABASE_ERROR";
            }
        }

        // DELETE CATEGORY
        // Deletes a category belonging to the specified event.
        public string deleteCategory(int eventID, int categoryID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Categories WHERE eventID=@eventID AND categoryID=@categoryID", con))
                {
                    // Supplies the event and category IDs.
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@categoryID", SqlDbType.Int).Value = categoryID;

                    con.Open();

                    // Returns SUCCESS if the category was deleted.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                // Saves the database error.
                message = ex.Message;

                // Error 547 indicates that related enrolments prevent deletion.
                return ex.Number == 547 ? "HAS_ENROLMENTS" : "DATABASE_ERROR";
            }
        }

        // DELETE PARTICIPANT ENROLMENT. A result must be removed first.
        // Deletes an enrolment only when it belongs to the specified participant
        // and no result is linked to that enrolment.
        public string deleteEnrolment(int enrolmentID, int userID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM Enrolments
                    WHERE enrolmentID=@enrolmentID AND userID=@userID
                    AND NOT EXISTS (SELECT 1 FROM Results WHERE Results.enrolmentID=Enrolments.enrolmentID)", con))
                {
                    // Supplies the enrolment ID and participant's user ID.
                    cmd.Parameters.Add("@enrolmentID", SqlDbType.Int).Value = enrolmentID;
                    cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

                    con.Open();

                    // Returns SUCCESS if deleted; otherwise the enrolment was not found
                    // for that user or a result is linked to it.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND_OR_HAS_RESULT";
                }
            }
            // Saves the SQL error and returns a database error status.
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // UPDATE RESULT by resultID. Ownership is verified by controller.
        // Updates the finishing time and position for a result.
        public string updateResult(int resultID, TimeSpan finishingTime, int finishingPosition)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Results
                    SET finishingTime=@time, finishingPosition=@position
                    WHERE resultID=@resultID", con))
                {
                    // Supplies the updated finishing time, position and result ID.
                    cmd.Parameters.Add("@time", SqlDbType.Time).Value = finishingTime;
                    cmd.Parameters.Add("@position", SqlDbType.Int).Value = finishingPosition;
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;

                    con.Open();

                    // Returns SUCCESS if the result was updated, otherwise NOT_FOUND.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            // Saves the SQL error and returns a database error status.
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // GET EVENT ID FOR RESULT
        // Finds the event associated with a particular result by joining
        // Results, Enrolments and Events.
        public int? getEventIDForResult(int resultID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"SELECT E.eventID
                    FROM Results R
                    INNER JOIN Enrolments EN ON R.enrolmentID=EN.enrolmentID
                    INNER JOIN Events E ON EN.eventID=E.eventID
                    WHERE R.resultID=@resultID", con))
                {
                    // Supplies the ID of the result being checked.
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;

                    con.Open();

                    // Retrieves the event ID linked to the result.
                    object? value = cmd.ExecuteScalar();

                    // Returns null if no matching event ID is found.
                    return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
                }
            }
            // Saves the SQL error and returns null if the lookup fails.
            catch (SqlException ex) { message = ex.Message; return null; }
        }

        // DELETE RESULT
        // Deletes a result using its result ID.
        public string deleteResult(int resultID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Results WHERE resultID=@resultID", con))
                {
                    // Supplies the ID of the result to delete.
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;

                    con.Open();

                    // Returns SUCCESS if deleted, otherwise NOT_FOUND.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            // Saves the SQL error and returns a database error status.
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

    }
}
