using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;

namespace EmployeeApi.Model
{
    public class EmployeeOperations : Database
    {
        public EmployeeOperations(IConfiguration config) : base(config)
        {
        }

        public override Employee GetEmployeeRecord(string employeeId)
        {
            Employee employee = null;

            this.QueryCommand = $"select * from EmployeeRepository where Id = '{employeeId}';";

            try
            {
                this.Connection.Open();

                this.Command.CommandText = this.QueryCommand;

                SqlDataReader reader = Command.ExecuteReader();

                if (reader.HasRows)
                {
                    employee = new Employee();
                    while (reader.Read())
                    {
                        employee.employeeId = Convert.ToString(reader["Id"]);
                        employee.emailId = Convert.ToString(reader["Email Id"]);
                        employee.employeeName = Convert.ToString(reader["Employee Name"]);
                        employee.mobileNumber = Convert.ToString(reader["Mobile Number"]);
                        employee.role = Convert.ToString(reader["Role"]);
                        employee.gender = Convert.ToString(reader["Gender"]);
                    }
                }
            }
            catch (SqlException error)
            {
                Console.WriteLine(error.Message);
            }
            finally
            {
                this.Connection.Close();
            }

            return employee;
        }


        public async Task SendEmail(Email email)
        {
            string sender = "taskify2023@outlook.com";
            string password = "Taskify@2023";
            //string recipientEmail = "barunkumar1197@gmail.com";
            //string subject = "Hello, Email!";
            //string body = "This is the body of the email.";

            MailMessage mail = new MailMessage(sender, email.recipientMail, email.subject, email.body);

            mail.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.outlook.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(sender, password),
                EnableSsl = true
            };

            

            await smtpClient.SendMailAsync(mail);
        }
    }
}
