namespace Race.Models
{//start of namespace
    public class CreateEnrolmentRequest
    {//start of class
        public int enrolmentID { get; set; }
        public DateTime enrolmentDate { get; set; }
        public int userID { get; set; }
        public int eventID { get; set; }
        public int categoryID { get; set; }
    }//end of class
}//end of namespace
