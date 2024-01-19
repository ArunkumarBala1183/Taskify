using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Management.Models.Templete
{
    public class Chat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ScaffoldColumn(false)]
        [BindNever]
        public int id {  get; set; }

        public int? projectId { get; set; }

        public string? personId { get; set; } 
        public string message { get; set; }
        public DateTime? created { get; set; }
    }
}
