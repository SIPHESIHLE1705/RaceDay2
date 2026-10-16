namespace Race.Models
{//start of namespace
    public class Category
    {// start of class
        public int categoryID { get; set; }
        public string categoryName { get; set; } = string.Empty;
        public int categoryAge { get; set; }
        public decimal categoryDistance { get; set; }
        public TimeSpan startTime { get; set; }
        public int eventID { get; set; }
    }//end of class
}//end of namespace 
