using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class Work
    {
        public int Id { get; set; }
        public WorkType Type { get; set; } = WorkType.Issue;
        [Required]
        public string In { get; set; }
        [Required]
        public string Out { get; set; }
        [Required]
        public string Date { get; set; }
        [StringLength(10)]
        public string Comment { get; set; }
        public virtual User User { get; set; }
        public string UserEmail { get; set; }
    }
}