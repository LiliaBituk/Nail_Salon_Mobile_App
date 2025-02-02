using SQLite;

namespace Business_Logic
{
    public class Customer
    {
        public int Id { get; set; }

        public string CustomerFullName { get; set; }

        public DateTime CustomerBirthDate { get; set; }

        public decimal CustomerPhoneNumber { get; set; }

        public bool CustomerIsNew { get; set; }

        public ICollection<VisitLogs> VisitLogs { get; set; }

        public bool IsRecordingSuccessful(bool isCustomerCreated, bool isEmployeeAvailable)
        {
            if (!isCustomerCreated)
            {
                throw new InvalidOperationException("Ошибка в записи клиента");
            }

            if (!isEmployeeAvailable)
            {
                throw new InvalidOperationException("Мастер занят в выбранное время");
            }

            return true;
        }
    }
}
