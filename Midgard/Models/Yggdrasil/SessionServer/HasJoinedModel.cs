using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Midgard.Models.Yggdrasil.SessionServer
{
    public class HasJoinedModel
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string ServerId { get; set; }
        
        public IPAddress IP { get; set; }
    }
}