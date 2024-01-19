using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Management.Models.Templete
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ScaffoldColumn(false)]
        public int projectId {get; set;}
       
        public string? description { get; set;}
        [Required (ErrorMessage ="Title is Required")]
        [MinLength (3 , ErrorMessage ="Title length should be minimum 2")]
        [MaxLength (30 , ErrorMessage = "Title length should be maximum 30")]
        public string title { get; set;}
        [ScaffoldColumn(false)]
        public string? managerId { get; set;}

        [DisplayFormat(DataFormatString ="{0:yyyy-MM-dd}" , ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }
        
        public bool closedProject { get; set; }
    }
}
