using System.ComponentModel.DataAnnotations;
using Midgard.Models.Yggdrasil.Shared;

namespace Midgard.Models.Yggdrasil.AuthServer
{
    public class AuthenticateModel
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        public string ClientToken { get; set; }
        
        public bool RequestUser { get; set; }
        
        public AgentModel Agent { get; set; }
    }
}