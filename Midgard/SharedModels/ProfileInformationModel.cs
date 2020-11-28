using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Midgard.SharedModels
{
    public class ProfileInformationModel
    {
        [Required]
        public string Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public List<PropertyModel> Properties { get; set; }
    }
}