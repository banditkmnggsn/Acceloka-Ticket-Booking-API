using MediatR;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.BookTicket
{
    public class BookTicketCommand : IRequest<BookedTicketResponse>
    {
        public List<TicketItemRequest> Tickets { get; set; } = new();
    }
}
