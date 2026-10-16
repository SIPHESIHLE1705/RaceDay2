namespace Race.Models
{//START OF NAMESAPCE
    public class CreateResultRequest
    {//START OF CLASS
        public int enrolmentID { get; set; }
        public TimeSpan finishingTime { get; set; }
        public int finishingPosition { get; set; }
    }//END OF CLASS
}//END OF NAMESPACE
