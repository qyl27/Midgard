using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Midgard.ViewModels.Yggdrasil.Index
{
    public class IndexViewModel
    {
        [Required]
        public string SignaturePublickey { get; set; }
        
        [Required]
        public List<string> SkinDomains { get; set; }
        
        [Required]
        public MetaViewModel Meta { get; set; }
    }
}