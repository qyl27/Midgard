using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Midgard.SharedModels
{
    public class TextureInformation
    {
        [Required]
        public long TimeStamp { get; set; }
        
        [Required]
        public string ProfileId { get; set; }
        
        [Required]
        public string ProfileName { get; set; }
        
        [Required]
        public Dictionary<string, SkinInformation> Textures { get; set; }
    }
}