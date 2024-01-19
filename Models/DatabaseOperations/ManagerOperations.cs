using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Task_Management.Models.CustomException;
using Task_Management.Models.DatabaseConnection;
using Task_Management.Models.Notification;
using Task_Management.Models.Templete;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task_Management.Models.DatabaseOperations
{
    public class ManagerOperations : DatabaseContext
    {
        private readonly EmployeeOperations _employee;
        private readonly IConfiguration _config;
        private readonly EmailService _email;
        public ManagerOperations(DbContextOptions<DatabaseContext> opts, EmployeeOperations employee, ILogger<ManagerOperations> logger, IConfiguration config, EmailService email) : base(opts)
        {
            _employee = employee;
            _config = config;
            _email = email;
        }

        public DbSet<Manager> manager { get; set; }


        public object authenticateUser(UserLogin user)
        {
            try
            {
                var managerDetails = manager.FirstOrDefault(manager => manager.managerId.ToLower() == user.userId.ToLower() && manager.password.ToLower() == user.password.ToLower());

                if (managerDetails != null)
                {
                    return managerDetails;
                }
                else
                {
                    var userDetails = _employee.Employees.FirstOrDefault(employee => employee.employeeId.ToLower() == user.userId.ToLower() && employee.password.ToLower() == user.password.ToLower());
                    if (userDetails != null)
                    {
                        return userDetails;
                    }
                }
            }
            catch (SqlException error)
            {
                Log.Error(error.Message);
                throw new AppException(_config.GetSection("Variables:ProjectController")["fail"]);
            }
            catch (Exception error)
            {
                Log.Error(error.Message);
                throw new AppException(_config.GetSection("Variables")["Connection Error"]);
            }

            return null;
        }

        public object searchData(string id)
        {
            try
            {
                var managerDetails = manager.Find(id);

                if (managerDetails != null)
                {
                    return managerDetails;
                }
            }
            catch (SqlException error)
            {
                Log.Error(error.Message);
            }
            return null;
        }

        private string generateOtp()
        {
            int length = 6;
            const string numbers = "0123456789";

            Random? random = new Random();

            char[] otp = new char[length];

            for (int i = 0; i < length; i++)
            {
                otp[i] = numbers[random.Next(numbers.Length)];
            }

            return new string(otp);
        }

        private object GetUserDetails(string id)
        {
            var managerData = this.searchData(id);

            if (managerData != null)
            {
                return managerData;
            }

            var employeeData = _employee.searchData(id);

            if (employeeData != null)
            {
                return employeeData;
            }

            return null;
        }

        public object GetId(object data)
        {
            try
            {
                string id = (string)data;

                Email? mailDetails = null;

                string otp = string.Empty;

                string emailId = string.Empty;

                string userId = string.Empty;

                var userDetails = GetUserDetails(id);

                if (userDetails == null)
                {
                    return null;
                }

                if(userDetails.GetType() == typeof(Manager))
                {
                    Manager? managerDetails = (Manager) userDetails;
                    emailId = managerDetails.emailId;
                }
                else
                {
                    Employee? employeeDetails = (Employee) userDetails;
                    emailId = employeeDetails.emailId;
                }

                otp = generateOtp();

                mailDetails = new Email()
                {
                    recipientMail = emailId,
                    subject = _config.GetSection("Variables:Email:Otp")["subject"],
                    body = _config.GetSection("Variables:Email:Otp")["body"].Replace("{otp}", otp)
                };


                if (mailDetails != null)
                {
                    _email.SendEmail(mailDetails);

                    var returnData = new { id = id, otp = otp };

                    return JsonConvert.SerializeObject(returnData);
                }

                return null;
                
            }
            catch (InvalidCastException)
            {
                ResetPassword? resetDetails = (ResetPassword) data;

                try
                {
                    var userDetails = GetUserDetails(resetDetails.id);

                    if (userDetails.GetType() == typeof(Manager))
                    {
                        Manager? managerDetails = manager.Find(resetDetails.id);
                        managerDetails.password = resetDetails.password;
                        this.SaveChanges();
                    }
                    else
                    {
                        Employee? employeeDetails = _employee.Employees.Find(resetDetails.id);
                        employeeDetails.password = resetDetails.password;
                        _employee.SaveChanges();
                    }

                }
                catch (SqlException error)
                {
                    Log.Error(error.Message);
                    throw new AppException(_config.GetSection("Variables:ProjectController")["fail"]);
                }
                catch (Exception error)
                {
                    Log.Error(error.Message);
                    throw new AppException(_config.GetSection("Variables")["Connection Error"]);
                }

                return true;
            }
            catch (SqlException error)
            {
                Log.Error(error.Message);
                throw new AppException(_config.GetSection("Variables:ProjectController")["fail"]);
            }
            catch (Exception error)
            {
                Log.Error(error.Message);
                throw new AppException(_config.GetSection("Variables")["Connection Error"]);
            }

        }
    }
}
