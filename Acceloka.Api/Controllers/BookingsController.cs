using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Acceloka.Api.Features.GetBookedTicket;
using Acceloka.Api.Features.RevokeTicket;
using Acceloka.Api.Features.EditBookedTicket;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IMediator mediator, ILogger<BookingsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("get-booked-ticket/{bookedTicketId}")]
        public async Task<IActionResult> GetBookedTicket(
            string bookedTicketId,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(bookedTicketId, out var parsedId))
            {
                throw new KeyNotFoundException($"Booking dengan ID {bookedTicketId} tidak ditemukan");
            }

            _logger.LogInformation("GetBookedTicket endpoint called for ID {BookedTicketId}", parsedId);

            var query = new GetBookedTicketQuery { BookedTicketId = parsedId };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("revoke-ticket/{bookedTicketId}/{kodeTiket}/{qty}")]
        [Authorize]
        public async Task<IActionResult> RevokeTicket(
            string bookedTicketId,
            string kodeTiket,
            int qty,
            CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(bookedTicketId, out var parsedId))
            {
                _logger.LogWarning("RevokeTicket: Invalid GUID format {BookedTicketId}", bookedTicketId);
                throw new KeyNotFoundException($"Booking dengan ID {bookedTicketId} tidak ditemukan");
            }

            _logger.LogInformation("RevokeTicket: Request for BookedTicketId {BookedTicketId}, KodeTiket {KodeTiket}, Qty {Qty}, User {UserId}", 
                parsedId, kodeTiket, qty, userId);

            var command = new RevokeTicketCommand
            {
                BookedTicketId = parsedId,
                KodeTiket = kodeTiket,
                Qty = qty,
                UserId = userId  // Set UserId from JWT
            };

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("RevokeTicket: Successfully revoked ticket for BookedTicketId {BookedTicketId}", parsedId);
            return Ok(result);
        }

        [HttpPut("edit-booked-ticket/{bookedTicketId}")]
        [Authorize]
        public async Task<IActionResult> EditBookedTicket(
            string bookedTicketId,
            [FromBody] List<TicketItemRequest> tickets,
            CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(bookedTicketId, out var parsedId))
            {
                _logger.LogWarning("EditBookedTicket: Invalid GUID format {BookedTicketId}", bookedTicketId);
                throw new KeyNotFoundException($"Booking dengan ID {bookedTicketId} tidak ditemukan");
            }

            _logger.LogInformation("EditBookedTicket: Request for BookedTicketId {BookedTicketId}, User {UserId}", 
                parsedId, userId);

            var command = new EditBookedTicketCommand
            {
                BookedTicketId = parsedId,
                Tickets = tickets,
                UserId = userId  // Set UserId from JWT
            };

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("EditBookedTicket: Successfully updated booking {BookedTicketId} for user {UserId}", 
                parsedId, userId);
            return Ok(result);
        }
    }
}
