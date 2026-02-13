namespace Acceloka.Api.Infrastructure.Persistence.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }

        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
        public decimal Price { get; set; }
        public int Quota { get; set; }

        public Category Category { get; set; } = null!;
        public ICollection<BookedTicketDetail> BookedTicketDetails { get; set; } = new List<BookedTicketDetail>();
    }
}
