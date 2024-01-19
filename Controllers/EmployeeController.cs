using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;
using Task_Management.Models.ActionFilter;
using Task_Management.Models.DatabaseConnection;
using Task_Management.Models.DatabaseOperations;
using Task_Management.Models.Templete;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Task_Management.Models.CustomException;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task_Management.Controllers
{
    [CustomActionFilter]
    public class EmployeeController : Controller
    {
        private readonly IDatabaseOperations _database;
        private readonly IConfiguration _config;
        private readonly EmployeeOperations _employee;

        public EmployeeController(IEnumerable<IDatabaseOperations> database , IConfiguration config , EmployeeOperations employee) 
        {
            _database = database.FirstOrDefault(s => s.GetType() == typeof(EmployeeOperations));
            _config = config;
            _employee = employee;
        }

        [HttpPost]
        public IActionResult GetEmployeeRecord(UserForm userForm)
        {
            try
            {
                if(userForm.employeeId != null)
                {
                    Employee employeeRecord = _employee.GetRecord(userForm.employeeId);

                    return RedirectToAction("FetchEmployee" , employeeRecord);
                }
                else
                {
                    TempData["empty"] = _config.GetSection("Variables:ProjectController")["empty"]; ;
                    return RedirectToAction("AddEmployee");
                }
            }
            catch (AppException error)
            {
                TempData["Head"] = _config.GetSection("Variables:Notification")["fail"];
                TempData["Error"] = error.Message;
                TempData["showToast"] = true;
                return RedirectToAction("Logout", "Login");
            }
        }
        public IActionResult AddEmployee()
        {
            Log.Information("Control => Add Employee Action");

            if (TempData["empty"] != null)
            {
                ViewBag.Empty = TempData["empty"];
                TempData.Clear();
                return View();
            }

            ViewBag.showToast = TempData["showToast"];
            ViewBag.Head = TempData["Head"];
            ViewBag.Message = TempData["Message"];

            Log.Information(ViewBag.Head);
            Log.Information(ViewBag.Message);

            TempData.Clear();

            return View();
        }

        public IActionResult FetchEmployee(Employee employee)
        {

            TempData["showToast"] = true;

            Log.Information("Fetched \n {@record}", employee);
            Log.Information("Before session");

            employee.projectId = (int)HttpContext.Session.GetInt32("projectId");
            employee.managerId = HttpContext.Session.GetString("managerId");
            
            try
            {
                var addedDetails = _database.insertData(employee);

                Log.Information($"\n\n\n{addedDetails.GetType().ToString()}\n\n\n");

                if (addedDetails.GetType() == typeof(bool))
                {
                    TempData["Head"] = "Success";
                    TempData["Message"] = _config.GetSection("Variables:EmployeeController")["success"];
                    Log.Information($"Success : {TempData["Message"]}");
                }

                return RedirectToAction("AddEmployee");
            }
            catch (AppException error)
            {
                TempData["Head"] = _config.GetSection("Variables:Notification")["fail"];
                TempData["Message"] = error.Message;
            }
            catch (Exception error)
            {
                TempData["Head"] = _config.GetSection("Variables:Notification")["fail"];
                TempData["Message"] = error.Message;
            }

            return RedirectToAction("AddEmployee");
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile csvFile)
        {
            
            TempData["showToast"] = true;
            
            if (csvFile != null)
            {
                CsvUploaded csvUploaded = new CsvUploaded();
                using (var streamReader = csvFile.OpenReadStream())
                using (var reader = new StreamReader(streamReader, Encoding.UTF8))
                {
                    while(!reader.EndOfStream)
                    {
                        csvUploaded.employeeIds.Add(reader.ReadLine());
                    }
                    
                };

                csvUploaded.projectId = (int)HttpContext.Session.GetInt32("projectId");
                csvUploaded.managerId = HttpContext.Session.GetString("managerId");

                try
                {
                    var response = await _employee.InsertFile(csvUploaded);

                    Log.Information($"{response.GetType().ToString()}");

                    TempData["Head"] = _config.GetSection("Variables:Notification")["sucess"];
                    TempData["Message"] = _config.GetSection("Variables:EmployeeController")["success"];
                    Log.Information($"Success : {TempData["Message"]}");
                }
                catch (AppException error)
                {
                    TempData["Head"] = _config.GetSection("Variables:Notification")["fail"];
                    TempData["Message"] = error.Message;
                }
            }
            else
            {
                TempData["Head"] = _config.GetSection("Variables:Notification")["fail"];
                TempData["Message"] = _config.GetSection("Variables:EmployeeController")["upload"];
            }

            return RedirectToAction("AddEmployee");

        }

        public IActionResult EditEmployee(string employeeId) 
        {
            Log.Information("Control => Edit Employee Action");
            var employeeDetails = _database.searchData(employeeId);

            Log.Information("Fetched \n{@Record}", employeeDetails);
            return View(employeeDetails);
        }

        [HttpPost]
        public IActionResult EditEmployee(Employee employee)
        {
            Log.Information("Control => Edit Employee Post");
            var updatedDetails = _database.updateData(employee);
            if((bool) updatedDetails)
            {
                return RedirectToAction("ViewEmployees");
            }
            ViewData["message"] = _config.GetSection("Variables:EmployeeController")["fail"];
            return View();
        }
        
        public IActionResult ViewEmployees(int linkCode) 
        {
            Log.Information($"Control =>View Employees Action {linkCode}");
            var employeeDetails = _database.viewData(HttpContext.Session.GetInt32("projectId"));
            
            if(employeeDetails.GetType() == typeof(List<Employee>))
            {
                ViewBag.linkCode = linkCode;
                Log.Information("Fetched \n{@Record}", employeeDetails);
                return View(employeeDetails);
            }
            ViewBag.status = employeeDetails;
            Log.Information("Fetched \n{@Record}", ViewBag.status);
            return View();
        }

        public IActionResult DeleteEmployee(string employeeId) 
        {
            _database.deleteData(employeeId);
            return RedirectToAction("ViewEmployees");
        }
    }
}
