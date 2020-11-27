using System;
using System.ComponentModel.DataAnnotations;
using Midgard.Enumerates;

namespace Midgard.DbModels
{
    public class Token
    {
        [Required]
        public Guid AccessToken { get; set; }
        
        [Required]
        public string ClientToken { get; set; }
        
        [Required]
        public DateTime IssueTime { get; set; }
        
        [Required]
        public DateTime ExpireTime { get; set; }
        
        [Required]
        public TokenStatus Status { get; set; }
        
        public virtual User BindUser { get; set; }

        public virtual Profile BindProfile { get; set; }
    }
}