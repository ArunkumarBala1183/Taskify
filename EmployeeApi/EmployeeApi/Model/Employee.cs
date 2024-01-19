using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Model
{
    public class Employee
    {
        public string employeeId { get; set; }
        public string employeeName { get; set; }

        public string role { get; set; }
        public string gender { get; set; }

        public string emailId { get; set; }
        public string mobileNumber { get; set; }
    }
}
