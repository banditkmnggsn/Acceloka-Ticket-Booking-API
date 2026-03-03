using MediatR;
using Acceloka.Api.Features.Auth.Dtos;

namespace Acceloka.Api.Features.Auth.Queries
{
    public class GetCurrentUserQuery : IRequest<UserDto>
    {
        public Guid UserId { get; set; }
    }
}
