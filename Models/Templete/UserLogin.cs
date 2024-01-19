using System.ComponentModel.DataAnnotations;
namespace Task_Management.Models.Templete
{
    public class UserLogin
    {
        [Required(ErrorMessage ="User Id is Empty")]
        public string userId { get; set; }

        [Required(ErrorMessage ="Password is Empty")]
        public string password { get; set; }
    }
}
