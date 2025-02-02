using SQLite;

namespace Business_Logic
{
    public class VisitLogs
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int CustomerId { get; set; } 
        public int ServiceId { get; set; }

       // public int EmployeeId { get; set; } 

        public DateTime StartDateTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal Price { get; set; }
    }
}
