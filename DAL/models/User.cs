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
        public virtual List<Work> Work { get; }
        public virtual List<Vacation> Vacation { get; }
        public virtual List<Break> Break { get; }
    }
}