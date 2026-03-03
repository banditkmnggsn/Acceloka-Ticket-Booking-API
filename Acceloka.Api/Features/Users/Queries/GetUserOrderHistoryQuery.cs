using MediatR;
using Acceloka.Api.Features.Users.Dtos;

namespace Acceloka.Api.Features.Users.Queries
{
    public class GetUserOrderHistoryQuery : IRequest<OrderHistoryResponse>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
