using System.ComponentModel.DataAnnotations;
using CET322.Data;

namespace CET322.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }

        public string? AssignedToId { get; set; }
        public ApplicationUser? AssignedTo { get; set; }

        [Required]
        public string Priority { get; set; } = "Düşük";  

        public int? StageId { get; set; }
        public Stage? Stage { get; set; }

        [Required]
        public string Status { get; set; } = "Açık";     

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<TicketNote>? Notes { get; set; }

        public ICollection<Comment>? Comments { get; set; }
    }
}
