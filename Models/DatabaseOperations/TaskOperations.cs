using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System.Dynamic;
using Task_Management.Models.CustomException;
using Task_Management.Models.DatabaseConnection;
using Task_Management.Models.Notification;
using Task_Management.Models.Templete;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task_Management.Models.DatabaseOperations
{
    public class TaskOperations : DatabaseConnection.DatabaseContext, IDatabaseOperations
    {
        private readonly EmployeeOperations _employee;
        private readonly IConfiguration _config;
        private readonly EmailService _email;
        public DbSet<Tasks> tasks { get; set; }
        public TaskOperations(DbContextOptions<DatabaseContext> opts , EmployeeOperations employee, IConfiguration config , EmailService email) : base(opts)
        {
            _employee = employee;
            _config = config;
            _email = email;
        }

        public void deleteData(object data)
        {
            int taskId = (int)data;
            Tasks task = (Tasks)this.searchData(taskId);
            if (task != null)
            {
                tasks.Remove(task);
                this.SaveChanges();
            }
        }

        public object insertData(object data)
        {
            Tasks task = (Tasks)data;
            try
            {
                task.employeeIds = JsonConvert.SerializeObject(task.selectedIds);
                task.createdDate = DateTime.Now;
                
                if(task.taskFile != null)
                {
                    task.uploadedFile = task.ConvertToByte(task.taskFile);
                    task.fileName = task.taskFile.FileName.ToString();
                }

                tasks.Add(task);
                this.SaveChanges();

                this.sendTaskNotification(task.selectedIds);
                return true;
            }
            catch (SqlException error)
            {
                Log.Error(error.Message);
                return false;
            }
        }

        public object searchData(object id)
        {
            int taskId = (int)id;
            Tasks task = tasks.Find(taskId);
            if (task != null) 
            {
                if (task.uploadedFile != null)
                {
                    task.selectAll = true;
                }

                task.selectedIds = JsonConvert.DeserializeObject<string[]>(task.employeeIds);
            }
            return task;
        }

        public object updateData(object data)
        {
            Tasks task = (Tasks)data;

            Tasks taskDetails = (Tasks) this.searchData(task.taskId);


            if (taskDetails != null)
            {
                taskDetails.Title = task.Title;
                taskDetails.Description = task.Description;
                taskDetails.submittedDate = task.submittedDate;

                if (task.taskFile != null)
                {
                    taskDetails.uploadedFile = task.ConvertToByte(task.taskFile);
                    taskDetails.fileName = task.taskFile.FileName.ToString();
                }

                if (task.selectedIds != null)
                {
                    taskDetails.employeeIds = JsonConvert.SerializeObject(task.selectedIds);
                }
            }

            try
            {
                this.SaveChanges();
                return true;
            }
            catch (SqlException error)
            {
                Log.Error(error.Message);
                return false;
            }
        }

        public object viewData(object id)
        {
            string managerId = (string) id;
            try
            {
                var taskDetails = tasks.Where(t=> t.managerId == id).ToList();
                return taskDetails;
            }
            catch(SqlException error)
            {
                Log.Error(error.Message);
                return false;
            }
            
        }

        public WrapperTask ViewDetails(int taskId)
        {
            dynamic models = new ExpandoObject();
            try
            {
                Tasks taskDetails = (Tasks)this.searchData(taskId);
                List<Employee> employeeDetails = new List<Employee>();

                foreach (string employeeId in taskDetails.selectedIds)
                {
                    employeeDetails.Add((Employee)_employee.searchData(employeeId));
                }

                models.tasks = taskDetails;
                models.employees = employeeDetails;

                WrapperTask wrapperTask = new WrapperTask();
                wrapperTask.tasksEmployees = models;

                return (wrapperTask);
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

        private void sendTaskNotification(string[] employeeIds)
        {
            foreach(string employeeId in employeeIds) 
            {
                Employee employee = (Employee)_employee.searchData(employeeId);

                string emailId = employee.emailId;

                Email mailDetails = new Email()
                {
                    recipientMail = emailId,
                    subject = _config.GetSection("Variables:Email:Task")["subject"],
                    body = _config.GetSection("Variables:Email:Task")["body"].Replace("{employeeId}" , employee.employeeId)
                };

                _email.SendEmail(mailDetails);
            }
        }
    }
}
