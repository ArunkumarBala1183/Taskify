using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Task_Management.Models.CustomException;
using Task_Management.Models.DatabaseConnection;
using Task_Management.Models.Templete;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task_Management.Models.DatabaseOperations
{
    public class ChatOperations : DatabaseContext
    {
        private readonly IConfiguration _config;
        public ChatOperations(DbContextOptions<DatabaseContext> opts, IConfiguration config, ILogger<ChatOperations> logger) : base(opts)
        {
            _config = config;
        }

        public DbSet<Chat> chat { get; set; }

        public void insertData(Chat chatDetails)
        {
            Log.Information("Insert Data Function Executed");
            try
            {
                chat.Add(chatDetails);
                this.SaveChanges();
                Log.Information("Chat added been added");
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

        public object viewData(int taskId)
        {
            try
            {
                var chatDetails = chat.Where(chat => chat.projectId == taskId).OrderByDescending(chat => chat.created).ToList();
                Log.Information("Chat had been retrived");

                if (chatDetails.Count > 0)
                {
                    return chatDetails;
                }
                Log.Information(_config.GetSection("Variables:ChatController")["empty"]);
                return _config.GetSection("Variables:ChatController")["empty"];
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