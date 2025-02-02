
using SQLite;

namespace Business_Logic
{
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string EmployeeFullName { get; set; }

        public decimal EmployeePhoneNumber { get; set; }

        public string EmployeeTypeService { get; set; }

        public string EmploymentContractNumber { get; set; }

        public DateTime EmployeeBirthDate { get; set; }

        public bool PermanentEmployeeStatus { get; set; }

        public int Score { get; set; }

    }
}
