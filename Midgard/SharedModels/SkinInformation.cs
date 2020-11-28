using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Midgard.SharedModels
{
    public class SkinInformation
    {
        [Required]
        public string Url { get; set; }
        
        [Required]
        public Dictionary<string, string> Metadata { get; set; }
    }
}