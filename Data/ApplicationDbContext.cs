using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CET322.Models;

namespace CET322.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketNote> TicketNotes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TicketNote ilişkileri
            builder.Entity<TicketNote>()
                .HasOne(n => n.Ticket)
                .WithMany(t => t.Notes)
                .HasForeignKey(n => n.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TicketNote>()
                .HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            //  Comment -> Ticket ilişkisi 
            builder.Entity<Comment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments) // ← 
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment -> User ilişkisi 
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
