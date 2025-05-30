using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CET322.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Stage> Stages { get; set; }  // Aşamalar ile ilişki
    }
}
