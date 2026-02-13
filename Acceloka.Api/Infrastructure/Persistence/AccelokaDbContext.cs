using Acceloka.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Api.Infrastructure.Persistence
{
    public class AccelokaDbContext : DbContext
    {
        public AccelokaDbContext(DbContextOptions<AccelokaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<BookedTicket> BookedTickets => Set<BookedTicket>();
        public DbSet<BookedTicketDetail> BookedTicketDetails => Set<BookedTicketDetail>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>()
                .HasIndex(x => x.TicketCode)
                .IsUnique();
        }
    }
}
