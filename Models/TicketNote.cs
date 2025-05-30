using CET322.Data;

namespace CET322.Models
{
    public class TicketNote
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string AuthorId { get; set; } = string.Empty; // Notu yazan user/admin
        public ApplicationUser? Author { get; set; }
    }
}