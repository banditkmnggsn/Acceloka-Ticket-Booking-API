using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Application.Services;
using Acceloka.Api.Features.Auth.Dtos;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Infrastructure.Persistence.Entities;

namespace Acceloka.Api.Features.Auth.Commands
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly AccelokaDbContext _dbContext;
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            AccelokaDbContext dbContext,
            IAuthService authService,
            ILogger<RegisterCommandHandler> logger)
        {
            _dbContext = dbContext;
            _authService = authService;
            _logger = logger;
        }

        public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already registered");
            }

            var userId = Guid.NewGuid();
            var passwordHash = _authService.HashPassword(request.Password);
            var refreshToken = _authService.GenerateRefreshToken();

            var user = new User
            {
                Id = userId,
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var accessToken = _authService.GenerateAccessToken(userId, user.Email, user.Name);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

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
