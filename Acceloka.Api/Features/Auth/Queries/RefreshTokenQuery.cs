using MediatR;
using Acceloka.Api.Features.Auth.Dtos;

namespace Acceloka.Api.Features.Auth.Queries
{
    public class RefreshTokenQuery : IRequest<RefreshTokenResponse>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
