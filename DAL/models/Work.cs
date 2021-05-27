using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class Work
    {
        public int Id { get; set; }
        public string Type { get; set; } = WorkType.Issue.ToString("G");
        [Required]
        public string In { get; set; }
        [Required]
        public string Out { get; set; }
        [Required]
        public string Date { get; set; }
        [StringLength(200)]
        public string Comment { get; set; }
    }
}