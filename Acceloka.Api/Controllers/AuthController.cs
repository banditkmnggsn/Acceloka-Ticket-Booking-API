using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Acceloka.Api.Features.Auth.Commands;
using Acceloka.Api.Features.Auth.Dtos;
using Acceloka.Api.Features.Auth.Queries;

namespace Acceloka.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Register request for email: {Email}", request.Email);

            var command = new RegisterCommand
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Login request for email: {Email}", request.Email);

            var query = new LoginQuery
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            _logger.LogInformation("Get current user: {UserId}", userId);

            var query = new GetCurrentUserQuery { UserId = userId };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            _logger.LogInformation("Logout request for user: {UserId}", userId);

            var command = new LogoutCommand { UserId = userId };
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Refresh token request");

            var query = new RefreshTokenQuery
            {
                RefreshToken = request.RefreshToken
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
