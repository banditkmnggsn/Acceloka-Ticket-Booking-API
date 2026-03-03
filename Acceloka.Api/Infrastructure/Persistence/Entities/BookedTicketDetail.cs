using System;

namespace Acceloka.Api.Infrastructure.Persistence.Entities
{
    public class BookedTicketDetail
    {
        public Guid Id { get; set; }

        public Guid BookedTicketId { get; set; }
        public Guid TicketId { get; set; }

        public int Quantity { get; set; }

        public BookedTicket BookedTicket { get; set; } = null!;
        public Ticket Ticket { get; set; } = null!;
    }
}