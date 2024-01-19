using Newtonsoft.Json;
using Serilog;
using System.Text;
using Task_Management.Models.CustomException;
using Task_Management.Models.Templete;

namespace Task_Management.Models.Notification
{
    public class EmailService
    {
        private string address;

        private HttpClient client;

        private readonly IConfiguration _config;
        
        public EmailService(IConfiguration config)
        {
            client = new HttpClient();
            _config = config;
            address = $"{_config.GetSection("Variables")["baseUri"]}{_config.GetSection("Variables:Api")["email"]}";
        }
        

        public async Task SendEmail(Email mailDetails)
        {
            try
            {
                Log.Information(address);

                string data = JsonConvert.SerializeObject(mailDetails);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync(address, content).Result;

                Log.Information($"{response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Log.Information($"{mailDetails.employeeId} Notification Sended");
                }
                else
                {
                    Log.Information($"{mailDetails.employeeId} => Invalid Email Address");
                }
            }
            catch (Exception)
            {

                throw new AppException("Connect to web Api");
            }
        }
    }
}
