using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Application.Services;
using Acceloka.Api.Features.Auth.Dtos;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Infrastructure.Persistence.Entities;

namespace Acceloka.Api.Features.Auth.Queries
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse>
    {
        private readonly AccelokaDbContext _dbContext;
        private readonly IAuthService _authService;
        private readonly ILogger<LoginQueryHandler> _logger;

        public LoginQueryHandler(
            AccelokaDbContext dbContext,
            IAuthService authService,
            ILogger<LoginQueryHandler> logger)
        {
            _dbContext = dbContext;
            _authService = authService;
            _logger = logger;
        }

        public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var refreshToken = _authService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var accessToken = _authService.GenerateAccessToken(user.Id, user.Email, user.Name);

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return new AuthResponse
            {
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                },
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600
            };
        }
    }
}
