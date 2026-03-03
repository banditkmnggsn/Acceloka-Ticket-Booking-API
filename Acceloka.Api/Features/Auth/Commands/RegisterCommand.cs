using MediatR;
using Acceloka.Api.Features.Auth.Dtos;

namespace Acceloka.Api.Features.Auth.Commands
{
    public class RegisterCommand : IRequest<AuthResponse>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
