using System.Text.Json.Serialization;

namespace Race.Models
{//START OF NAMSAPCE
    public class User
    {//START OF CLASS
        public int userID { get; set; }
        public string fullName { get; set; } = string.Empty;
        public string emailAddress { get; set; } = string.Empty;

        [JsonIgnore]
        public string password { get; set; } = string.Empty;

        public string userRole { get; set; } = string.Empty;
    }//END OF CLASS
}//START OF NAMESPACE
