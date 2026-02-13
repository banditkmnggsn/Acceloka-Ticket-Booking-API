using MediatR;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.GetBookedTicket
{
    public class GetBookedTicketQuery : IRequest<List<BookedTicketDetailResponse>>
    {
        public Guid BookedTicketId { get; set; }
    }
}
