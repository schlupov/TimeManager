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
        public List<Work> Work { get; }
        public List<Vacation> Vacation { get; }
        public List<Break> Break { get; }
        public User()
        {
            Work = new List<Work>();
            Vacation = new List<Vacation>();
            Break = new List<Break>();
        }
    }
}