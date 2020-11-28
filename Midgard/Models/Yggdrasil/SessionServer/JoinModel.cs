using System;
using System.ComponentModel.DataAnnotations;

namespace Midgard.Models.Yggdrasil.SessionServer
{
    public class JoinModel
    {
        [Required]
        public Guid AccessToken { get; set; }
        
        [Required]
        public Guid SelectedProfile { get; set; }
        
        [Required]
        public string ServerId { get; set; }
    }
}