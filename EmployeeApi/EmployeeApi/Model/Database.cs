using System.Data.SqlClient;

namespace EmployeeApi.Model
{
    public abstract class Database
    {
        private readonly IConfiguration _config;
        public SqlConnection Connection { get; set; }

        public SqlCommand Command { get; set; }

        public string QueryCommand = "";

        public string ConnectionCredentials;

        public Database(IConfiguration config) 
        {
            Connection = new SqlConnection();
            Command = new SqlCommand();
            Command.Connection = Connection;
            ConnectionCredentials = "";
            _config = config;
        }

        public void ConnectDatabase()
        {
            ConnectionCredentials = _config.GetConnectionString("connection1");
            this.Connection.ConnectionString = ConnectionCredentials;
        }

        public abstract Employee GetEmployeeRecord(string employeeId);
    }
}
