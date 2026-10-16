namespace Race.Models
{/START OF NAMESPACE
    public class CreateCategoryRequest
    {//START OF CLASS
        public string categoryName { get; set; } = string.Empty;
        public int categoryAge { get; set; }
        public decimal categoryDistance { get; set; }
        public TimeSpan startTime { get; set; }
    }//END OF CLASS
}//EDN OF NAMESPACE
