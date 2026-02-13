using MediatR;
using Microsoft.AspNetCore.Mvc;
using Acceloka.Api.Features.GetAvailableTicket;
using Acceloka.Api.Features.BookTicket;
using Acceloka.Api.Features.Common.Dtos;

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
            _logger.LogInformation("GetAvailableTicket endpoint called");

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

            return Ok(result);
        }

        [HttpPost("book-ticket")]
        public async Task<IActionResult> BookTicket(
            [FromBody] BookTicketCommand command,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("BookTicket endpoint called");

            var result = await _mediator.Send(command, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
