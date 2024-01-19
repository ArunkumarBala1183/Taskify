using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Data;
using Task_Management.Models.DatabaseConnection;
using Task_Management.Models.Templete;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Serilog;
using Task_Management.Models.CustomException;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Concurrent;
using Task_Management.Models.Notification;

namespace Task_Management.Models.DatabaseOperations
{
    public class EmployeeOperations : DatabaseContext, IDatabaseOperations
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmployeeOperations> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly EmailService _email;

            public EmployeeOperations(DbContextOptions<DatabaseContext> opts, IConfiguration config, ILogger<EmployeeOperations> logger, IServiceProvider serviceProvider, EmailService email) : base(opts)
        {
            _config = config;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _email = email;
        }

        public DbSet<Employee> Employees { get; set; }

        public void deleteData(object data)
        {
            try
            {
                string employeeId = (string)data;
                Employee employeeDetails = Employees.Find(employeeId);
                if (employeeDetails != null)
                {
                    Employees.Remove(employeeDetails);
                    this.SaveChanges();
                }
                _logger.LogInformation("Employee Deleted Executed");
            }
            catch (SqlException error)
            {
                _logger.LogError(error, "SQL Connection Error Occurs at Delete Data");
            }
        }

        public object insertData(object data)
        {
            _logger.LogInformation("Inserted Data Function Executed");
            string[] roles = _config.GetSection("Variables:Roles").Get<string[]>();
            Employee employee = (Employee)data;
            employee.role = roles[1];
            employee.password = Guid.NewGuid().ToString();


            try
            {
                return this.InsertDetails(employee);
            }
            catch (AppException)
            {
                throw;
            }
        }

        public object InsertDetails(Employee employee)
        {
            try
            {
                Employees.Add(employee);
                this.SaveChanges();
                _logger.LogInformation($"{employee.employeeId} Details Added");

                Email mailDetails = new Email()
                {
                    employeeId = employee.employeeId,
                    recipientMail = employee.emailId,
                    subject = _config.GetSection("Variables:Email:Account")["subject"],
                    body = _config.GetSection("Variables:Email:Account")["body"].Replace("{employeeId}" , employee.employeeId).Replace("{password}", employee.password)
                };

                _email.SendEmail(mailDetails);

                return true;
            }
            catch (DbUpdateException error)
            {
                _logger.LogInformation("Employee Details Exists");
                throw new AppException(_config.GetSection("Variables:EmployeeController")["exists"]);
            }
            catch (SqlException error)
            {
                Log.Error(error.Message);
                throw new AppException(_config.GetSection("Variables:ProjectController")["fail"]);
            }
            catch (Exception error)
            {
                Log.Error(error.Message);
                throw new AppException(_config.GetSection("Variables:EmployeeController")["invalid"]);
            }
        }

        public async Task<object> InsertFile(object data)
        {
            List<Task> tasks = new List<Task>();

            var emailDetails = new ConcurrentBag<Employee>();

            Log.Information("Insert File Inside came");

            CsvUploaded csvData = (CsvUploaded)data;

            StringBuilder existingId = new StringBuilder();

            string notValidId = string.Empty;

            Log.Information("List Data \n {@result}", csvData.employeeIds);

            foreach (string id in csvData.employeeIds)
            {
                Employee employee = this.GetRecord(id);

                Log.Information("After getting record");

                Log.Information("Record \n {@record} \n", employee);

                if (employee != null)
                {
                    employee.projectId = csvData.projectId;
                    employee.managerId = csvData.managerId;
                    employee.password = Guid.NewGuid().ToString();

                    Log.Information("before task add");

                    tasks.Add(Task.Run(async () =>
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var employeeDbcontext = scope.ServiceProvider.GetService<EmployeeOperations>();

                            try
                            {
                                await employeeDbcontext.Employees.AddAsync(employee);
                                employeeDbcontext.SaveChanges();

                                emailDetails.Add(employee);

                                _logger.LogInformation($"{employee.employeeId} Details Added");
                            }
                            catch (DbUpdateException)
                            {
                                Log.Information($"{id} already member");
                                existingId.Append(id);
                                throw new AppException(existingId.ToString());
                            }
                            catch (SqlException error)
                            {
                                Log.Information("Returning Sql Error");
                                Log.Error(error.Message);
                                throw;
                            }
                        }
                    }));
                }
                else
                {
                    notValidId = notValidId + $"{id}\n";
                }
            }

            try
            {
                await Task.WhenAll(tasks);

                Log.Information("\n\n\n Details {@record}\n\n", emailDetails);

                if (!notValidId.IsNullOrEmpty())
                {
                    Log.Information("Returning not valid");
                    throw new AppException(notValidId);
                }
                else
                {
                    Log.Information("Returning object");
                    return true;
                }

            }
            catch (AggregateException ex) when (ex.InnerExceptions.Any(error => error is AppException))
            {
                Log.Information($"Exception : {ex.Message}...Already a member");
                throw new Exception($"{existingId.ToString()}");
            }
            finally
            {
                await this.sendEmailDetails(emailDetails);
            }

        }

        private async Task sendEmailDetails(ConcurrentBag<Employee> emailDetails)
        {
            foreach (var employee in emailDetails)
            {
                Email mailDetails = new Email()
                {
                    employeeId = employee.employeeId,
                    recipientMail = employee.emailId,
                    subject = _config.GetSection("Variables:Email:Account")["subject"],
                    body = _config.GetSection("Variables:Email:Account")["body"].Replace("{employeeId}", employee.employeeId).Replace("{password}", employee.password)
                };

                await _email.SendEmail(mailDetails);
            }
        }

        public List<string> GetFile(IFormFile csv)
        {

            List<string> employeeId = new List<string>();

            using (var fileStream = csv.OpenReadStream())
            using (var reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();
                    employeeId.Add(row);
                }
            }

            return employeeId;
        }

        public object searchData(object id)
        {
            try
            {
                string employeeId = (string)id;
                var employeeDetails = Employees.Find(employeeId);
                if (employeeDetails != null)
                {
                    _logger.LogInformation("Data had been Searched");
                    return employeeDetails;
                }
            }
            catch (SqlException error)
            {
                _logger.LogError(error, "SQL Connection Error at SearchData");
            }
            return null;
        }

        public object updateData(object data)
        {
            Employee employee = (Employee)data;
            Employee employeeDetails = (Employee)this.searchData(employee.employeeId);
            if (employeeDetails != null)
            {
                employeeDetails.employeeName = employee.employeeName;
                employeeDetails.emailId = employee.emailId;
                employeeDetails.mobileNumber = employee.mobileNumber;
            }

            try
            {
                this.SaveChanges();
                return true;
            }
            catch (SqlException error)
            {
                _logger.LogError(error, "SQL Connection Error at Update Data");
                return false;
            }
        }

        public object viewData(object id)
        {
            int projectId = (int)id;

            try
            {
                var employeeDetails = Employees.Where(e => e.projectId == projectId).ToList();

                if (employeeDetails.Count > 0)
                {
                    return employeeDetails;
                }
            }
            catch (SqlException error)
            {
                _logger.LogError(error, "SQL Connection Error at ViewData");
            }
            return _config.GetSection("Variables:EmployeeController")["empty"];
        }

        public Employee GetRecord(string employeeId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string address = $"{_config.GetSection("Variables")["baseUri"]}{_config.GetSection("Variables:Api")["search"] + employeeId}";

                    Log.Information(address);

                    HttpResponseMessage response = client.GetAsync(address).Result;

                    Log.Information($"{response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {

                        string result = response.Content.ReadAsStringAsync().Result;
                        Log.Information("Details \n {@record}", result);
                        Employee employee = JsonConvert.DeserializeObject<Employee>(result);
                        return employee;
                    }

                    return null;
                }
            }
            catch (Exception)
            {
                throw new AppException("connect Web api");
            }
        }

    }
}
