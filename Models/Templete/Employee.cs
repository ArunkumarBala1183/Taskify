using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using Newtonsoft.Json;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.RegularExpressions;

namespace Task_Management.Models.Templete
{
    public class Employee
    {
        [Key]
        [Required(ErrorMessage = "Employee Id is Required")]
        [ScaffoldColumn(false)]
        public string employeeId { get; set; }
        public int? projectId { get; set; }
        [Required(ErrorMessage = "Name is Required")]
        [MinLength(3, ErrorMessage = "Name should be Minimum 3 letters")]
        [MaxLength(30, ErrorMessage = "Name should with in 30 letters")]
        public string employeeName { get; set; }

        public string? role { get; set; }
        [Required(ErrorMessage = "Gender is Required")]
        public string gender { get; set; }

        [Required(ErrorMessage = "Email Id is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string emailId { get; set; }
        public string? password { get; set; }
        public string? managerId { get; set; }
        [ValidateMobileNumber(ErrorMessage = "Not a valid number")]
        public string mobileNumber { get; set; }

        [NotMapped]
        public IFormFile? csvFile { get; set; }

        [NotMapped]
        public DataTable? employeeDetails { get; set; }

        public List<string> GetFile()
        {

            List<string> employeeId = new List<string>();

            var fileStream = this.csvFile.OpenReadStream();
            var reader = new StreamReader(fileStream);

            while (!reader.EndOfStream)
            {
                string row = reader.ReadLine();
                employeeId.Add(row);
            }


            return employeeId;

            //Log.Information("After File Reader");
            //employeeDetails = new DataTable();
            //employeeDetails.Columns.Add("Employee Id");
            //employeeDetails.Columns.Add("Name");
            //employeeDetails.Columns.Add("Email Id");
            //employeeDetails.Columns.Add("Mobile Number");
            //employeeDetails.Columns.Add("Gender");



            //Log.Information(row);

            //while ((row = reader.ReadLine()) is not null)
            //{
            //    int index = 0;
            //    employeeDetails.Rows.Add();
            //    foreach (string column in row.Split(","))
            //    {
            //        employeeDetails.Rows[employeeDetails.Rows.Count - 1][index] = column;
            //        index++;
            //    }
            //}


            //foreach (DataRow values in employeeDetails.Rows)
            //{
            //    Console.WriteLine(values["Employee Id"]);
            //}

            //return employeeDetails;
        }
    }

    public class ValidateMobileNumber : ValidationAttribute
    {
        private const string pattern = @"^[6-9]\d{9}$";
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string number = (string)value;

                if (Regex.IsMatch(number, pattern))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
