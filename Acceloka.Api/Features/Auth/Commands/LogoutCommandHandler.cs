using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;

namespace Acceloka.Api.Features.Auth.Commands
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly AccelokaDbContext _dbContext;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            AccelokaDbContext dbContext,
            ILogger<LogoutCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var refreshTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == request.UserId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
            }

            _dbContext.RefreshTokens.UpdateRange(refreshTokens);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User logged out and refresh tokens revoked: {UserId}", request.UserId);

            return Unit.Value;
        }
    }
}
