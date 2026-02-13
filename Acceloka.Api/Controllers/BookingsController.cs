using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> RevokeTicket(
            string bookedTicketId,
            string kodeTiket,
            int qty,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(bookedTicketId, out var parsedId))
            {
                throw new KeyNotFoundException($"Booking dengan ID {bookedTicketId} tidak ditemukan");
            }

            _logger.LogInformation("RevokeTicket endpoint called");

            var command = new RevokeTicketCommand
            {
                BookedTicketId = parsedId,
                KodeTiket = kodeTiket,
                Qty = qty
            };

            var result = await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpPut("edit-booked-ticket/{bookedTicketId}")]
        public async Task<IActionResult> EditBookedTicket(
            string bookedTicketId,
            [FromBody] List<TicketItemRequest> tickets,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(bookedTicketId, out var parsedId))
            {
                throw new KeyNotFoundException($"Booking dengan ID {bookedTicketId} tidak ditemukan");
            }

            _logger.LogInformation("EditBookedTicket endpoint called");

            var command = new EditBookedTicketCommand
            {
                BookedTicketId = parsedId,
                Tickets = tickets
            };

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}
