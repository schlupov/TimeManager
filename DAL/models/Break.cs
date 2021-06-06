using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class Break
    {
        public int Id { get; set; }
        public BreakType Type { get; set; }
        [Required]
        public DateTime In { get; set; }
        [Required]
        public DateTime Out { get; set; }
        public virtual Work Work { get; set; }
    }
}