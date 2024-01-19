using EmployeeApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers
{
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeOperations _database;
        public EmployeeController(EmployeeOperations database)
        {
            _database = database;
        }

        [Route("GetEmployee/{employeeId}")]
        [HttpGet]
        public IActionResult GetEmployee(string employeeId) 
        {
            _database.ConnectDatabase();

            //_database.SendEmail();
            Employee employee = _database.GetEmployeeRecord(employeeId);

            if(employee == null) 
            {
                return BadRequest(); 
            }

            return Ok(employee);
        }

        [Route("SendEmail")]
        [HttpPost]
        public async Task<IActionResult> SendMail(Email mail)
        {
            try
            {
                await _database.SendEmail(mail);
                return Ok("Email Send Successfully");
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return BadRequest();
            }
        }
    }
}
