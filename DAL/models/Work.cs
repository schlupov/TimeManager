using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class Work
    {
        public int Id { get; set; }
        public string Type { get; set; } = WorkType.Issue.ToString("G");
        [Required]
        public DateTime In { get; set; }
        [Required]
        public DateTime Out { get; set; }
        [Required]
        public string Date { get; set; }
        [StringLength(200)]
        public string Comment { get; set; }
        public virtual Break Break { get; set; }
    }
}