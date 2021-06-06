using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.models
{
    public class Vacation
    {
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}