namespace Task_Management.Models.Templete
{
    public class Projects
    {
       public List<Project> currentProjects { get; set; }
       public List<Project> closedProjects { get; set; }

        public Projects()
        {
            currentProjects = new List<Project>();
            closedProjects = new List<Project>();
        }
    }
}
