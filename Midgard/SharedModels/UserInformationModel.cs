using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Midgard.SharedModels
{
    public class UserInformationModel
    {
        [Required]
        public string Id { get; set; }
        
        public List<PropertyModel> Properties { get; set; }
    }
}