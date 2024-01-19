using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Task_Management.Models.DatabaseConnection;
using Task_Management.Models.Templete;
using Task_Management.Models.ActionFilter;
using Task_Management.Models.CustomException;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task_Management.Models.DatabaseOperations
{
    public class ProjectOpertions : DatabaseContext, IDatabaseOperations
    {
        private readonly IConfiguration _config;
        public ProjectOpertions(DbContextOptions<DatabaseContext> opts, IConfiguration config) : base(opts)
        {
            _config = config;
        }
        protected DbSet<Project> project { get; set; }


        public object insertData(object details)
        {
            Project projectDetails = (Project)details;
            projectDetails.closedProject = false;
            projectDetails.createdDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            try
            {
                project.Add(projectDetails);
                this.SaveChanges();
                return true;
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

        public void deleteData(object data)
        {
            throw new NotImplementedException();
        }

        public object updateData(object data)
        {
            Project updatedDetails = (Project)data;
            try
            {
                Project projectDetails = project.Find(updatedDetails.projectId);

                projectDetails.title = updatedDetails.title;
                projectDetails.description = updatedDetails.description;

                this.SaveChanges();
                return true;
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

        public object viewData(object id)
        {
            string managerId = (string)id;

            try
            {
                var fetchedData = project.Where(p => p.managerId == managerId).ToList();

                if (fetchedData == null)
                {
                    return null;
                }

                Projects projects = new Projects();

                foreach (Project project in fetchedData as IEnumerable<Project>)
                {

                    if (project.closedProject)
                    {
                        projects.closedProjects.Add(project);
                    }
                    else
                    {
                        projects.currentProjects.Add(project);
                    }
                }
                return projects;
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

        public object searchData(object id)
        {
            try
            {
                string value = (string)id;

                var result = project.Where(project => project.title.Contains(value)).ToList();

                if (result.Count > 0)
                {
                    string message = result.Count + _config.GetSection("Variables:ProjectController")["records"];
                    return new { message = message, result = result };
                }
                else
                {
                    return _config.GetSection("Variables:ProjectController")["emptyRecords"];

                }
            }
            catch (InvalidCastException)
            {
                int projectId = (int)id;

                try
                {
                    var projectDetails = project.Find(projectId);

                    if (projectDetails != null)
                    {
                        return projectDetails;
                    }

                    return null;
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

        public void closeProject(int projectId)
        {
            try
            {
                Project projectDetails = project.Find(projectId);
                projectDetails.closedProject = true;

                this.SaveChanges();
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

        public void reteriveProject(int projectId)
        {
            try
            {
                Project projectDetails = project.Find(projectId);
                projectDetails.closedProject = false;

                this.SaveChanges();
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
