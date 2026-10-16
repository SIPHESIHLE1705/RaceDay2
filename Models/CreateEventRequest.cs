namespace Race.Models
{//START OF NAMESPACE
    public class CreateEventRequest
    {//START OF CLASS
        public string eventName { get; set; } = string.Empty;
        public string eventDescription { get; set; } = string.Empty;
        public string eventLocation { get; set; } = string.Empty;
        public decimal eventDistance { get; set; }
        public DateTime eventDate { get; set; }
        public string eventType { get; set; } = string.Empty;
    }// END OF CLASS
}//END OF NMAESPACE
