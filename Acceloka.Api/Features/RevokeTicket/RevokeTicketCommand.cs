using MediatR;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.RevokeTicket
{
    public class RevokeTicketCommand : IRequest<RevokeTicketResponse>
    {
        public Guid BookedTicketId { get; set; }
        public string KodeTiket { get; set; } = string.Empty;
        public int Qty { get; set; }
        public Guid? UserId { get; set; }  // From JWT claims - for validation
    }
}
