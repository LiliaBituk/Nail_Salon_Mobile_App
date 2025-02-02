using SQLite;

namespace Business_Logic
{
    public class Customer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CustomerFullName { get; set; }

        public DateTime CustomerBirthDate { get; set; }

        public decimal CustomerPhoneNumber { get; set; }

        public bool CustomerIsNew { get; set; }

    }
}
