using MediatR;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.EditBookedTicket
{
    public class EditBookedTicketCommand : IRequest<EditBookedTicketResponse>
    {
        public Guid BookedTicketId { get; set; }
        public List<TicketItemRequest> Tickets { get; set; } = new();
        public Guid? UserId { get; set; }  // From JWT claims - for validation
    }
}
