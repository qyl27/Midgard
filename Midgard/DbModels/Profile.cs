using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Midgard.Enumerates;

namespace Midgard.DbModels
{
    public class Profile
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        [RegularExpression("^[a-zA-Z0-9_]")]
        public string Name { get; set; }
        
        [Required]
        public bool IsSelected { get; set; }
        
        public virtual Skin Skin { get; set; }

        public virtual Skin Cape { get; set; }

        public virtual User Owner { get; set; }
        
        public virtual List<Token> BindTokens { get; set; }
        
        public virtual List<Session> ActiveSessions { get; set; }
        
        public Guid SkinId { get; set; }
        
        public Guid CapeId { get; set; }
    }
}