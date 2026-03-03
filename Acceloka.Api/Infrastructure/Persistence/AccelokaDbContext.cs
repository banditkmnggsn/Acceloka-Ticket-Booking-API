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
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique constraint pada TicketCode
            modelBuilder.Entity<Ticket>()
                .HasIndex(x => x.TicketCode)
                .IsUnique();

            // Index untuk analytics by booking date
            modelBuilder.Entity<BookedTicket>()
                .HasIndex(x => x.BookingDate);

            // CHECK constraint: Quota harus > 0
            modelBuilder.Entity<Ticket>()
                .ToTable(t => t.HasCheckConstraint("CK_Tickets_Quota_Positive", "\"Quota\" > 0"));

            // CHECK constraint: Quantity harus > 0 di BookedTicketDetails
            modelBuilder.Entity<BookedTicketDetail>()
                .ToTable(t => t.HasCheckConstraint("CK_BookedTicketDetails_Quantity_Positive", "\"Quantity\" > 0"));

            // Unique constraint pada User Email
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            // User relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.BookedTickets)
                .WithOne(bt => bt.User)
                .HasForeignKey(bt => bt.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade delete configuration (explicit)
            modelBuilder.Entity<BookedTicketDetail>()
                .HasOne(btd => btd.BookedTicket)
                .WithMany(bt => bt.Details)
                .HasForeignKey(btd => btd.BookedTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookedTicketDetail>()
                .HasOne(btd => btd.Ticket)
                .WithMany(t => t.BookedTicketDetails)
                .HasForeignKey(btd => btd.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
