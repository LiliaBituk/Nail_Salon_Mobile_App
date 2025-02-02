namespace Business_Logic
{
    public class VisitLogs
    {
        public int Id { get; set; }

        public Customer Customer { get; set; }

        public Service Service { get; set; }

        public Employee Employee { get; set; }

        public DateTime StartDateTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public decimal Price { get; set; }
    }
}
