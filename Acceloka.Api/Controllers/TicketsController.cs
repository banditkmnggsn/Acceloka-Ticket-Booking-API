using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Acceloka.Api.Features.GetAvailableTicket;
using Acceloka.Api.Features.BookTicket;
using Acceloka.Api.Features.Common.Dtos;
using Acceloka.Api.Features.AdminTickets.CreateTicket;
using Acceloka.Api.Features.AdminTickets.UpdateTicket;
using Acceloka.Api.Features.Categories.GetCategories;

namespace Acceloka.Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class TicketsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(IMediator mediator, ILogger<TicketsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("get-available-ticket")]
        public async Task<IActionResult> GetAvailableTicket(
            [FromQuery] string? namaKategori,
            [FromQuery] string? kodeTiket,
            [FromQuery] string? namaTiket,
            [FromQuery] decimal? harga,
            [FromQuery] DateTime? tanggalEventMinimal,
            [FromQuery] DateTime? tanggalEventMaksimal,
            [FromQuery] string orderBy = "KodeTiket",
            [FromQuery] string orderState = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetAvailableTicket: Request with filters - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            var query = new GetAvailableTicketQuery
            {
                NamaKategori = namaKategori,
                KodeTiket = kodeTiket,
                NamaTiket = namaTiket,
                HargaMaksimal = harga,
                TanggalEventMinimal = tanggalEventMinimal,
                TanggalEventMaksimal = tanggalEventMaksimal,
                OrderBy = orderBy,
                OrderState = orderState,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);

            _logger.LogInformation("GetAvailableTicket: Successfully retrieved {Count} tickets", result.Count());
            return Ok(result);
        }

        [HttpGet("categories/list")]
        [Authorize]
        public async Task<IActionResult> GetCategories(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpPost("book-ticket")]
        [Authorize]
        public async Task<IActionResult> BookTicket(
            [FromBody] BookTicketCommand command,
            CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            _logger.LogInformation("BookTicket: Request with {TicketCount} ticket types for user {UserId}",
                command.Tickets?.Count ?? 0, userId);

            command.UserId = userId;  // Set UserId from JWT
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("BookTicket: Successfully created booking {BookedTicketId} for user {UserId}",
                result.BookedTicketId, userId);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("admin/tickets")]
        [Authorize]
        public async Task<IActionResult> CreateTicket(
            [FromBody] CreateTicketRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateTicketCommand
            {
                TicketCode = request.TicketCode,
                TicketName = request.TicketName,
                CategoryId = request.CategoryId,
                EventDate = request.EventDate,
                Price = request.Price,
                Quota = request.Quota,
                ImageUrl = request.ImageUrl,
                Description = request.Description
            };

            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("admin/tickets/{ticketCode}")]
        [Authorize]
        public async Task<IActionResult> UpdateTicket(
            [FromRoute] string ticketCode,
            [FromBody] UpdateTicketRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateTicketCommand
            {
                CurrentTicketCode = ticketCode,
                TicketCode = request.TicketCode,
                TicketName = request.TicketName,
                CategoryId = request.CategoryId,
                EventDate = request.EventDate,
                Price = request.Price,
                Quota = request.Quota,
                ImageUrl = request.ImageUrl,
                Description = request.Description
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
