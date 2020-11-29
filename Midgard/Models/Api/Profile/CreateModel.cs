using System.ComponentModel.DataAnnotations;

namespace Midgard.Models.Api.Profile
{
    public class CreateModel
    {
        [Required]
        [RegularExpression("[a-zA-Z0-9_]*")]
        public string Name { get; set; }
    }
}