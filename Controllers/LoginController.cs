using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Task_Management.Models.JWT;
using Task_Management.Models.Templete;
using Microsoft.AspNetCore.Http;
using NuGet.Common;
using Task_Management.Models.DatabaseOperations;
using Task_Management.Models.CustomException;
using Serilog;
using Task_Management.Models.ActionFilter;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Operations;

namespace Task_Management.Controllers
{
    [CustomActionFilter]
    public class LoginController : Controller
    {
        private readonly IConfiguration _config;
        private readonly JWTOperations _token;
        private readonly ManagerOperations _manager;

        public LoginController(IConfiguration config , JWTOperations token , ManagerOperations manager)
        {
            _config = config;
            _token = token;
            _manager = manager;
        }

        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            ViewBag.Head = TempData["Head"];
            ViewBag.Error = TempData["Error"];
            ViewBag.showToast = TempData["showToast"];

            TempData.Clear();
            Log.Information("Login Action Executed");
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLogin user)
        {
            Log.Information("Control => Login Post");

            if(!ModelState.IsValid)
            {
                Log.Information("Validation Error \n{@Record}", ModelState.Values);
                return View();
            }
            
            try
            {
                var userDetails = _manager.authenticateUser(user);

                if (userDetails != null && userDetails.GetType() == typeof(Manager))
                {
                    Manager manager = (Manager)userDetails;
                    var token = _token.generateToken(manager);

                    HttpContext.Session.SetString("token", token);
                    HttpContext.Session.SetString("managerId", manager.managerId);

                    return RedirectToAction("AuthenticateRole");
                }
                else if (userDetails != null)
                {
                    Employee employee = (Employee)userDetails;
                    var token = _token.generateToken(employee);

                    HttpContext.Session.SetString("token", token);
                    HttpContext.Session.SetString("EmployeeId", employee.employeeId);

                    return RedirectToAction("AuthenticateRole");
                }
                else
                {
                    ViewBag.Head = "Warning";
                    ViewBag.Error = "Unauthorized Access";
                }
            }
            catch (AppException error) 
            {
                ViewBag.Head = "Fail";
                ViewBag.Error = error.Message;
            }
            Log.Warning("Authentication Failed");

            ViewBag.showToast = true;

            return View();
        }

        public IActionResult ForgottenPassword()
        {
            return View();
        }

        
        [HttpPost]
        public IActionResult ForgottenPassword(ResetPassword reset)
        {
            Log.Information("Inside Forgotten Password");

            Log.Information($"Id fetched {reset.id}");

            if(!ModelState.IsValid)
            {
                Log.Information("{@record}", ModelState.Values);

                Log.Information("Inside Model state");
                return View();
            }

            string? otpSended = HttpContext.Session.GetString("otp");
            string? otpreceived = HttpContext.Session.GetString("userotp");

            Log.Information($"otpsended : {otpSended}");

            try
            {
                if (otpSended == null)
                {
                    string userDetails = (string)_manager.GetId(reset.id);

                    if (userDetails == null)
                    {
                        Log.Information("Otp is null");
                        ViewBag.status = "Invalid UserId";
                    }
                    else
                    {
                        dynamic userString = JsonConvert.DeserializeObject<object>(userDetails);
                        Log.Information("{@userDetails} is detected" , userDetails);

                        HttpContext.Session.SetString("userId", (string)userString.id);
                        HttpContext.Session.SetString("otp", (string) userString.otp);
                        HttpContext.Session.SetString("skipError", "success");

                        Log.Information($"Otp is fetched at session {HttpContext.Session.GetString("otp")}");
                        ViewBag.allowOtp = true;

                        ViewBag.showToast = true;
                        ViewBag.Head = "Success";
                        ViewBag.Message = "Otp sended";
                    }
                }
                else if (string.Equals(otpSended, reset.otp))
                {
                    HttpContext.Session.SetString("userotp", reset.otp);
                    Log.Information($"otpsended : {otpSended}, otp : {reset.otp}");
                    ViewBag.allowOtp = false;

                    ViewBag.otpStatus = true;

                    ViewBag.showToast = true;
                    ViewBag.Head = "Success";
                    ViewBag.Message = "Otp Verified";
                    
                    Log.Information("Otp verified");
                    
                    ViewBag.showPassword = true;

                    HttpContext.Session.Remove("skipError");
                    //HttpContext.Session.Remove("otp");
                }
                else if(string.Equals(otpSended, otpreceived))
                {
                    if ((HttpContext.Session.GetString("skipError") == null))
                    {
                        if (reset.IsPasswordMatch().GetType() == typeof(string))
                        {
                            ViewBag.showToast = true;
                            ViewBag.Head = "Fail";
                            ViewBag.Message = reset.IsPasswordMatch();
                            ViewBag.showPassword = true;

                            Log.Information("Custom exception goes here");
                        }
                        else
                        {
                            reset.id = HttpContext.Session.GetString("userId");
                            _manager.GetId(reset);
                            HttpContext.Session.Clear();
                            ViewBag.showToast = true;
                            ViewBag.Head = "Success";
                            ViewBag.Message = "Password has been changed \n please Login again";
                        }
                    }
                }
                else
                {
                    Log.Information($"otpsended : {otpSended}, otp : {reset.otp}");
                    Log.Information("otp not verified goes here");
                    ViewBag.allowOtp = true;
                    ViewBag.otpStatus = false;
                    
                    ViewBag.showToast = true;
                    ViewBag.Head = "Fail";
                    ViewBag.Message = "Otp not Verified";

                    Log.Information("Otp is not verified");
                }

                Log.Information("Before View Page renders");
            }
            catch (AppException error)
            {
                ViewBag.showToast = true;
                ViewBag.Head = "Fail";
                ViewBag.Message = error.Message;
                Log.Information($"Exception : \n {error.Message}\n");
            }

            return View();
        }

        public IActionResult AuthenticateRole()
        {
            Log.Information("Control => Authentication Role Action");
            string token = HttpContext.Session.GetString("token");

            string[] roles = _config.GetSection("Variables:Roles").Get<string[]>();
            var claims = _token.validateToken(token);

            if (claims != null && claims.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == roles[0]))
            {
                return RedirectToAction("ViewProjects", "Project");
            }

            else if(claims != null && claims.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == roles[1]))
            {
                return RedirectToAction("ViewEmployeeTasks", "Task");
            }

            Log.Warning("Authentication Failed");
            return BadRequest();
        }

        public IActionResult GetProfile([FromServices] ManagerOperations managerService, [FromServices] IEnumerable<IDatabaseOperations> database)
        {
            string? token = HttpContext.Session.GetString("token");

            string? userId = string.Empty;

            var validationResponse = _token.validateRole(token);

            if(validationResponse.GetType() == typeof(bool))
            {
                if((bool) validationResponse)
                {
                    userId = HttpContext.Session.GetString("managerId");
                    
                    var userDetails = managerService.searchData(userId);

                    return View();
                }
                else
                {
                    IDatabaseOperations? userService;
                    userId = HttpContext.Session.GetString("EmployeeId");
                    
                    userService = database.FirstOrDefault(service => service.GetType() == typeof(EmployeeOperations));

                    var userDetails = userService.searchData(userId);

                    return View(userDetails);

                }
            }

            return RedirectToAction("Logout");
        }

        [HttpPost]
        public IActionResult UpdateProfile()
        {
            return View();
        }

        public IActionResult Logout()
        {
            Log.Information("Control => Logout Action");
            HttpContext.Session.Clear();

            return RedirectToAction("Login","Login");
        }
    }
}
