using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Features.Auth.Dtos;
using Acceloka.Api.Infrastructure.Persistence;

namespace Acceloka.Api.Features.Auth.Queries
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
    {
        private readonly AccelokaDbContext _dbContext;
        private readonly ILogger<GetCurrentUserQueryHandler> _logger;

        public GetCurrentUserQueryHandler(
            AccelokaDbContext dbContext,
            ILogger<GetCurrentUserQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new KeyNotFoundException($"User not found: {request.UserId}");
            }

            _logger.LogInformation("Get current user: {UserId}", request.UserId);

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
