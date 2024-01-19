using Microsoft.AspNetCore.Mvc;
using Task_Management.Models.Templete;
using Task_Management.Models.DatabaseConnection;
using Task_Management.Models.DatabaseOperations;
using Microsoft.AspNetCore.Authorization;
using Task_Management.Models.JWT;
using System.Security.Claims;
using Task_Management.Models.ActionFilter;
using Task_Management.Models.CustomException;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Serilog;

namespace Task_Management.Controllers
{
    [CustomActionFilter]
    public class ProjectController : Controller
    {
        private readonly IDatabaseOperations _database;
        private readonly JWTOperations _token;
        private readonly IConfiguration _config;
        private readonly ProjectOpertions _project;
        public ProjectController(IEnumerable<IDatabaseOperations> database, JWTOperations token , IConfiguration config, ProjectOpertions project)
        {
            _database = database.FirstOrDefault(s => s.GetType() == typeof(ProjectOpertions));
            _token = token;
            _config = config;

            Log.Information("Project Controller  Executed");
            _project = project;
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ViewProjects()
        {
            Log.Information("Control => View Projects Action");

            string token = HttpContext.Session.GetString("token");

            HttpContext.Session.Remove("linkcode");

            var role = _token.validateRole(token);

            if (role.GetType() == typeof(bool) && (bool)role)
            {
                try
                {
                    var projectDetails = _database.viewData(HttpContext.Session.GetString("managerId"));
                    Log.Information("Fetched \n{@Record}", projectDetails);
                    return View(projectDetails);
                }
                catch (AppException error)
                {
                    TempData["Head"] = "Fail";
                    TempData["Error"] = error.Message;
                }
            }
            else
            {
                Log.Warning("Unauthorized Access");
            }

            TempData["showToast"] = true;

            return RedirectToAction("Logout" , "Login");
        }

        public IActionResult AddProject()
        {
            Log.Information("Control => Add Project Action");

            ViewBag.Head = TempData["Head"];
            ViewBag.Message = TempData["Message"];
            Log.Information("Success : {@Record}", ViewBag.Message);
            ViewBag.showToast = TempData["showToast"];

            TempData.Clear();

            return View();
        }

        [HttpPost]
        public IActionResult AddProject(Project project)
        {
            Log.Information("Control => Add Project Post");

            if (!ModelState.IsValid)
            {
                return View();
            }

            project.managerId = HttpContext.Session.GetString("managerId");

            TempData["showToast"] = true;

            try
            {
                var addedDetails = _database.insertData(project);

                if ((bool)addedDetails)
                {
                    TempData["Head"] = "Success";
                    TempData["Message"] = _config.GetSection("Variables:ProjectController")["success"];
                    Log.Information($"Success : {TempData["Message"]}");
                }

                return RedirectToAction("AddProject");
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;
                return RedirectToAction("Logout", "Login");
            }
        }

        [HttpPost]
        public IActionResult SearchProject()
        {
            Log.Information("Control => Search Project Action");
            string value = Request.Form["search"];

            if (value == "")
            {
                TempData["message"] = _config.GetSection("Variables:ProjectController")["empty"];
                return RedirectToAction("ViewProjects");
            }

            try
            {
                var searchedProject = _database.searchData(value);

                if (searchedProject.GetType() == typeof(string))
                {
                    TempData["message"] = (string)searchedProject;
                    return RedirectToAction("ViewProjects");
                }

                var anonymousType = searchedProject.GetType();
                TempData["message"] = (string)anonymousType.GetProperty("message").GetValue(searchedProject);

                var projects = anonymousType.GetProperty("result")?.GetValue(searchedProject);

                Log.Information("Fetched \n{@Record}", projects);
                return View("ViewProjects", projects);
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;
                TempData["showToast"] = true;
                return RedirectToAction("Logout", "Login");
            }

        }

        public IActionResult EditProject(int projectId)
        {
            Log.Information("Control => Edit Project Action");
            Project projectDetails = (Project)_database.searchData(projectId);

            Log.Information("Fetched \n{@Record}", projectDetails);
            return View(projectDetails);
        }

        [HttpPost]
        public IActionResult EditProject(Project project)
        {
            Log.Information("Control => Edit Project Post");

            try
            {
                var editedStatus = _database.updateData(project);

                if (editedStatus.GetType() == typeof(bool))
                {
                    return RedirectToAction("ViewProjects");
                }
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;
            }

            TempData["showToast"] = true;

            return RedirectToAction("Logout", "Login");
        }

        public IActionResult CloseProject(int projectId)
        {
            Log.Information("Control => Close Project Action");

            try
            {
                _project.closeProject(projectId);

                return RedirectToAction("ViewProjects");
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;
            }

            TempData["showToast"] = true;

            return RedirectToAction("Logout", "Login");
        }

        public IActionResult ReteriveProject(int projectId)
        {
            Log.Information("Control => Reterive Project Action");

            try
            {
                _project.reteriveProject(projectId);

                return RedirectToAction("ViewProjects");
            }
            catch (AppException error)
            {
                TempData["Head"] = "Fail";
                TempData["Error"] = error.Message;

            }

            TempData["showToast"] = true;

            return RedirectToAction("Logout", "Login");
        }
    }
}