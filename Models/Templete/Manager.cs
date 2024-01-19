using System.ComponentModel.DataAnnotations;

namespace Task_Management.Models.Templete
{
    public class Manager
    {
        [Key]
        public string managerId { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public string emailId { get; set; }
        public string role { get; set; }
        public string? password { get; set; }

        [ValidateMobileNumber(ErrorMessage = "Not a valid number")]
        public string mobileNumber { get; set; }
    }
}
