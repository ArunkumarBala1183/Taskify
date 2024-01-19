using System.ComponentModel.DataAnnotations;

namespace Task_Management.Models.Templete
{
    public class ResetPassword
    {
        //[Required(ErrorMessage ="Id is Required")]
        public string? id { get; set; }
        public string? otp { get; set; }
        public string? password { get; set; }
        public string? conformPassword { get; set; }


        public object IsPasswordMatch()
        {
            if(password == null || conformPassword == null)
            {
                return "Fill the Feilds";
            }

            return string.Equals(password , conformPassword) ? true : "Password and Conform Password does'nt Matched";
        }
    }
}
