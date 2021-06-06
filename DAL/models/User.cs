using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class User
    {
        [Key]
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public ICollection<Work> Work { get; set; }
        public ICollection<Vacation> Vacation { get; set; } 
    }
}