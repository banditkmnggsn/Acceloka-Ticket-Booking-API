using System;
using System.Collections.Generic;

namespace Acceloka.Api.Infrastructure.Persistence.Entities
{
    public class BookedTicket
    {
        public Guid Id { get; set; }
        public DateTime BookingDate { get; set; }

        public ICollection<BookedTicketDetail> Details { get; set; } = new List<BookedTicketDetail>();
    }
}
