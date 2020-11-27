using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Midgard.DbModels
{
    public class Session
    {
        [Required]
        public string ServerId { get; set; }
        
        [Required]
        public Guid AccessToken { get; set; }

        public IPAddress ClientIp { get; set; }
        
        [Required]
        public DateTime ExpireTime { get; set; }
        
        public virtual Profile BindProfile { get; set; }
    }
}