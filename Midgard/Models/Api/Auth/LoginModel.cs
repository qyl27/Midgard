using System.ComponentModel.DataAnnotations;

namespace Midgard.Models.Api.Auth
{
    public class LoginModel
    {
        [Required]
        [RegularExpression("[a-zA-Z0-9_]*")]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        public string Recaptcha { get; set; }
    }
}