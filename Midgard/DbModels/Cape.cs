using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Midgard.Enumerates;

namespace Midgard.DbModels
{
    public class Cape
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        public string Url { get; set; }
        
        public virtual User Owner { get; set; }
        
        public virtual List<Profile> UsedProfiles { get; set; }
    }
}