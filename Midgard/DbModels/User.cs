using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Midgard.Enumerates;

namespace Midgard.DbModels
{
    public class User
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        [RegularExpression("^[a-zA-Z0-9_]")]
        public string Username { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        public string PasswordSalt { get; set; }

        [Required]
        public bool IsEmailVerified { get; set; }

        [Required]
        public Permission Permission { get; set; }
        
        public virtual List<Profile> Profiles { get; set; }
        public virtual List<Token> Tokens { get; set; }
    }
}