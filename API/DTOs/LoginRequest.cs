using System.ComponentModel.DataAnnotations;


namespace API.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string username { get; set; }
        
        [Required]
        public string password { get; set; }
    }
}