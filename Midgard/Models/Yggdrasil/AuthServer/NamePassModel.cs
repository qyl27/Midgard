using System.ComponentModel.DataAnnotations;

namespace Midgard.Models.Yggdrasil.AuthServer
{
    public class NamePassModel
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}