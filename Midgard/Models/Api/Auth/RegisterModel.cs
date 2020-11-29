using System.ComponentModel.DataAnnotations;

namespace Midgard.Models.Api.Auth
{
    public class RegisterModel
    {
        [Required]
        [RegularExpression("[a-zA-Z0-9_]*")]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Recaptcha { get; set; }
    }
}