using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class Vacation
    {
        public int Id { get; set; }
        [Required]
        public string Date { get; set; }
        public string UserEmail { get; set; }
    }
}