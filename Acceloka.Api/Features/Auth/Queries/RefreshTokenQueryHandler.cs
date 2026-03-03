using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Application.Services;
using Acceloka.Api.Features.Auth.Dtos;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Infrastructure.Persistence.Entities;

namespace Acceloka.Api.Features.Auth.Queries
{
    public class RefreshTokenQueryHandler : IRequestHandler<RefreshTokenQuery, RefreshTokenResponse>
    {
        private readonly AccelokaDbContext _dbContext;
        private readonly IAuthService _authService;
        private readonly ILogger<RefreshTokenQueryHandler> _logger;

        public RefreshTokenQueryHandler(
            AccelokaDbContext dbContext,
            IAuthService authService,
            ILogger<RefreshTokenQueryHandler> logger)
        {
            _dbContext = dbContext;
            _authService = authService;
            _logger = logger;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
        {
            var refreshTokenEntity = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

            if (refreshTokenEntity == null || refreshTokenEntity.IsRevoked || refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token is invalid or expired");
            }

            var user = refreshTokenEntity.User;
            var newAccessToken = _authService.GenerateAccessToken(user.Id, user.Email, user.Name);
            var newRefreshToken = _authService.GenerateRefreshToken();

            refreshTokenEntity.IsRevoked = true;
            _dbContext.RefreshTokens.Update(refreshTokenEntity);

            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refresh token generated for user: {UserId}", user.Id);

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600
            };
        }
    }
}
