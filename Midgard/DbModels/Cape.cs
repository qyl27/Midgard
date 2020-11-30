using System;
using System.ComponentModel.DataAnnotations;

namespace Midgard.DbModels
{
    public class Cape
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Url { get; set; }
        
        public virtual User Owner { get; set; }
    }
}