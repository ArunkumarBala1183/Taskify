namespace Task_Management.Models.Templete
{
    public class CsvUploaded
    {
        public List<string> employeeIds { get; set; }

        public int projectId { get; set; }
        public string managerId { get; set; }

        public CsvUploaded()
        {
            employeeIds = new List<string>();
        }
    }
}
