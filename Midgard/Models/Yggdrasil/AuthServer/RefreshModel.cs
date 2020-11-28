using System;
using System.ComponentModel.DataAnnotations;
using Midgard.SharedModels;

namespace Midgard.Models.Yggdrasil.AuthServer
{
    public class RefreshModel
    {
        [Required]
        public Guid AccessToken { get; set; }
        
        public ProfileInformationModel SelectedProfile { get; set; }
        
        public bool RequestUser { get; set; }
        
        public string ClientToken { get; set; }
    }
}