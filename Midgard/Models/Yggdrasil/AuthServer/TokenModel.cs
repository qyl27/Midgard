using System;
using System.ComponentModel.DataAnnotations;

namespace Midgard.Models.Yggdrasil.AuthServer
{
    public class TokenModel
    {
        [Required]
        public Guid AccessToken { get; set; }
        
        public string ClientToken { get; set; }
    }
}