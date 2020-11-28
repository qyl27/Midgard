using System.ComponentModel.DataAnnotations;

namespace Midgard.SharedModels
{
    public class PropertyModel
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Value { get; set; }
        
        public string Signature { get; set; }
    }
}