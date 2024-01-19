using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Management.Models.Templete
{
    public class Tasks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int taskId { get; set; }
        public int? projectId { get; set; }
        public string? managerId { get; set; }
        public string? employeeIds { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public string? fileName { get; set; }
        
        public DateTime? createdDate { get; set; }
        public DateTime? deadlineDate { get; set; }
        public DateTime? submittedDate { get; set; }

        public byte[]? uploadedFile { get; set; }

        [NotMapped]
        public bool selectAll { get; set; }

        [NotMapped]
        public string[]? selectedIds { get; set; }
        [NotMapped]
        public IFormFile? taskFile { get; set; }

        public byte[] ConvertToByte(IFormFile file)
        {
            using (var memoryStream = new MemoryStream()) 
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

    }    
} 
