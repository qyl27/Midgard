using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Midgard.Models.Api.Skin
{
    public class AddSkinModel
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public bool IsSlim { get; set; }
        
        [Required]
        public IFormFile File { get; set; }
    }
}