using MediatR;

namespace Acceloka.Api.Features.Auth.Commands
{
    public class LogoutCommand : IRequest<Unit>
    {
        public Guid UserId { get; set; }
    }
}
