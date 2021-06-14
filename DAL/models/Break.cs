using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.models
{
    public class Break
    {
        public int Id { get; set; }
        public BreakType Type { get; set; }
        [Required]
        public string In { get; set; }
        [Required]
        public string Out { get; set; }
        [Required]
        public string Date { get; set; }
        public string UserEmail { get; set; }
    }
}