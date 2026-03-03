using MediatR;
using Acceloka.Api.Features.Auth.Dtos;

namespace Acceloka.Api.Features.Auth.Queries
{
    public class LoginQuery : IRequest<AuthResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
