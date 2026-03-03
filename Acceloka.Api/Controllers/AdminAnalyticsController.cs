using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Acceloka.Api.Features.AdminAnalytics.GetTicketSalesAnalytics;

namespace Acceloka.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/analytics")]
    [Authorize]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminAnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("ticket-sales")]
        public async Task<IActionResult> GetTicketSales(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string groupBy = "day",
            [FromQuery] int top = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetTicketSalesAnalyticsQuery
            {
                From = from,
                To = to,
                GroupBy = groupBy.ToLowerInvariant(),
                Top = top
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
