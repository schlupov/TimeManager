using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class User
    {
        [Key]
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}