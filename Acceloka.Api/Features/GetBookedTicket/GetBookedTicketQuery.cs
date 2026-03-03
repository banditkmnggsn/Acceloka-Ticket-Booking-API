using MediatR;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.GetBookedTicket
{
    public class GetBookedTicketQuery : IRequest<GetBookedTicketGroupedResponse>
    {
        public Guid BookedTicketId { get; set; }
    }
}
