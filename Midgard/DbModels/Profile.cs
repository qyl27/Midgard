using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Midgard.Enumerates;

namespace Midgard.DbModels
{
    public class Profile
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        
        [Required]
        [RegularExpression("[a-zA-Z0-9_]*")]
        public string Name { get; set; }
        
        [Required]
        public bool IsSelected { get; set; }

        [Required]
        public virtual User Owner { get; set; }
        
        public virtual Skin Skin { get; set; }
        
        public virtual Cape Cape { get; set; }
        
        public virtual List<Token> BindTokens { get; set; }
        
        public virtual List<Session> ActiveSessions { get; set; }
    }
}