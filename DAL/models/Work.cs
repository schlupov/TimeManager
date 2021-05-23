using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class Work
    {
        public int Id { get; set; }
        public WorkType Type { get; set; }
        [Required]
        public string In { get; set; }
        [Required]
        public string Out { get; set; }
        public string Date { get; set; }
        [StringLength(200)]
        public string Comment { get; set; }
    }
}