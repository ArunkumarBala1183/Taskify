using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Serilog;
using System.Dynamic;
using System.Security.Claims;
using Task_Management.Models.ActionFilter;
using Task_Management.Models.CustomException;
using Task_Management.Models.DatabaseOperations;
using Task_Management.Models.JWT;
using Task_Management.Models.Templete;

namespace Task_Management.Controllers
{
    [CustomActionFilter]
    public class TaskController : Controller
    {
        private readonly IDatabaseOperations _employee;
        private readonly IDatabaseOperations _task;
        private readonly ChatOperations _chat;
        private readonly TaskOperations _taskcontext;
        private readonly JWTOperations _token;
        private readonly IConfiguration _config;
        private readonly ManagerOperations _manager;

        public TaskController
            (
            IEnumerable<IDatabaseOperations> employee,
            IEnumerable<IDatabaseOperations> task,
            ChatOperations chat,
            TaskOperations taskcontext,
            ManagerOperations manager,
            JWTOperations token,
            IConfiguration config
            )
        {
            _employee = employee.FirstOrDefault(s => s.GetType() == typeof(EmployeeOperations));
            _task = task.FirstOrDefault(e => e.GetType() == typeof(TaskOperations));
            _chat = chat;
            _taskcontext = taskcontext;
            _token = token;
            _config = config;
            _manager = manager;
        }
        public IActionResult CreateTask()
        {
            Log.Information("Create Task Action Executed");
            var employeeDetails = _employee.viewData(HttpContext.Session.GetInt32("projectId"));
            if (employeeDetails.GetType() == typeof(List<Employee>))
            {
                ViewBag.employees = employeeDetails;
            }
            ViewBag.status = employeeDetails;
            Log.Information("Fetched \n{@Record}", employeeDetails);
            return View();
        }
        [HttpPost]
        public IActionResult CreateTask(Tasks task)
        {
            Log.Information("Create Task Post Executed");
            if (task.selectedIds != null)
            {
                task.projectId = (int)HttpContext.Session.GetInt32("projectId");
                task.managerId = HttpContext.Session.GetString("managerId");
                var createdMessage = _task.insertData(task);

                if ((bool)createdMessage)
                {
                    TempData["message"] = _config.GetSection("Variables:TaskController")["success"];
                }
                else
                {
                    TempData["message"] = _config.GetSection("Variables:TaskController")["fail"];
                }
            }
            return RedirectToAction("CreateTask");

        }
        public IActionResult EditTask(int taskId)
        {
            Log.Information("Edit Task Action Executed");
            Tasks taskDetails = (Tasks)_task.searchData(taskId);
            var employeeDetails = _employee.viewData(HttpContext.Session.GetInt32("projectId"));
            if (employeeDetails.GetType() == typeof(List<Employee>))
            {
                ViewBag.employees = employeeDetails;
            }
            ViewBag.status = employeeDetails;
            return View(taskDetails);
        }

        [HttpPost]
        public IActionResult EditTask(Tasks task)
        {
            Log.Information("Edit Task Post Executed");
            if (task.selectedIds != null)
            {
                var editedMessage = _task.updateData(task);

                if ((bool)editedMessage)
                {
                    return RedirectToAction("ViewTasks");
                }
                else
                {
                    ViewData["message"] = _config.GetSection("Variables:TaskController")["fail"];
                }
            }
            return View(task);
        }
        public IActionResult DeleteTask(int taskId)
        {
            Log.Information("Delete Task Action Executed");
            _task.deleteData(taskId);
            return RedirectToAction("ViewTasks");
        }
        public IActionResult ViewTasks(int projectId, bool isClosed)
        {
            Log.Information("ViewTask Action Executed");

            if (projectId != 0)
            {
                HttpContext.Session.SetInt32("projectId", projectId);

                if (isClosed)
                {
                    HttpContext.Session.SetInt32("linkCode", 1);
                }
                else
                {
                    HttpContext.Session.SetInt32("linkCode", 0);
                }
            }

            TempData["linkCode"] = HttpContext.Session.GetInt32("linkCode");

            var taskDetails = _task.viewData(HttpContext.Session.GetString("managerId"));

            if (taskDetails.GetType() == typeof(List<Tasks>) && ((List<Tasks>)taskDetails).Count > 0)
            {
                return View(taskDetails);
            }
            else if (taskDetails.GetType() == typeof(List<Tasks>))
            {
                ViewData["message"] = _config.GetSection("Variables:TaskController")["empty"];
            }
            else
            {
                ViewData["message"] = _config.GetSection("Variables:TaskController")["fail"];
            }
            return View();
        }

        public IActionResult ViewDetails(int projectId, int taskId)
        {
            try
            {
                WrapperTask wrapperTask = _taskcontext.ViewDetails(taskId);

                ViewBag.linkCode = TempData["linkCode"];
                TempData.Clear();

                var role = _token.validateRole(HttpContext.Session.GetString("token"));

                string[] roles = _config.GetSection("Variables:Roles").Get<string[]>();

                if (role.GetType() == typeof(bool) && (bool)role)
                {
                    ViewData["Role"] = roles[0];
                    ViewBag.userId = HttpContext.Session.GetString("managerId");
                }
                else
                {
                    ViewData["Role"] = roles[1];
                    ViewBag.userId = HttpContext.Session.GetString("EmployeeId");
                }

                return View(wrapperTask);
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;
            }

            TempData["showToast"] = true;

            return RedirectToAction("Logout", "Login");
        }

        public IActionResult DownloadFile(int taskId)
        {
            Log.Information("File Download Executed");
            Tasks taskDetails = (Tasks)_task.searchData(taskId);
            string contentType = _config.GetSection("Variables:TaskController")["content-type"];

            return File(taskDetails.uploadedFile, contentType, taskDetails.fileName);
        }

        [HttpPost]
        public IActionResult InsertChat(string message , int taskId)
        {
            Log.Information("Insert Chat Action Executed");
            var role = _token.validateRole(HttpContext.Session.GetString("token"));
            Chat chatDetails = new Chat()
            {
                message = message,
                projectId = taskId,
                created = DateTime.Now
            };

            if (role.GetType() == typeof(bool) && (bool)role)
            {
                chatDetails.personId = HttpContext.Session.GetString("managerId");
            }
            else
            {
                chatDetails.personId = HttpContext.Session.GetString("EmployeeId");
            }

            try
            {
                _chat.insertData(chatDetails);

                return Json(chatDetails);
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;
            }

            TempData["showToast"] = true;

            return RedirectToAction("Logout", "Login");
        }

        [HttpGet]
        public IActionResult ViewChats(int taskId)
        {
            Log.Information("View Chat Action Executed");
            try
            {
                var chatDetails = _chat.viewData(taskId);

                if (chatDetails.GetType() == typeof(List<Chat>))
                {
                    ViewBag.chats = chatDetails;
                }
                else
                {
                    ViewBag.message = chatDetails;
                }
                return Json(chatDetails);
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;
            }

            TempData["showToast"] = true;

            return RedirectToAction("Logout", "Login");
        }

        [HttpPost]
        public IActionResult GetChatPerson(string personId)
        {
            Log.Information("Get Chat Person Action Executed");
            try
            {
                Manager managerName = (Manager)_manager.searchData(personId);

                if (managerName != null)
                {
                    return Json(managerName.name);
                }

                Employee chatPerson = (Employee)_employee.searchData(personId);
                return Json(chatPerson.employeeName);
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;
            }

            TempData["showToast"] = true;

            return RedirectToAction("Logout", "Login");
        }


        public IActionResult ViewEmployeeTasks()
        {
            Log.Information("ViewEmployeeTask Action Executed");
            
            List<Tasks> taskDetails = _taskcontext.tasks.ToList();

            List<Tasks> employeeTask = new List<Tasks>();

            foreach (Tasks task in taskDetails)
            {
                task.selectedIds = JsonConvert.DeserializeObject<string[]>(task.employeeIds);

                if (task.selectedIds.Contains(HttpContext.Session.GetString("EmployeeId")))
                {
                    employeeTask.Add(task);
                }
            }
            Employee currentUser = (Employee) _employee.searchData(HttpContext.Session.GetString("EmployeeId"));

            HttpContext.Session.SetInt32("projectId", (int)currentUser.projectId);
          
            return View(employeeTask);
        }
    }
}
