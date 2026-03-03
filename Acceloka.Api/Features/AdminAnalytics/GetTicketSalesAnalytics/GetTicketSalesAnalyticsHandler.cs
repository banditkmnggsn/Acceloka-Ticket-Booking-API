using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;

namespace Acceloka.Api.Features.AdminAnalytics.GetTicketSalesAnalytics
{
    public class GetTicketSalesAnalyticsHandler : IRequestHandler<GetTicketSalesAnalyticsQuery, TicketSalesAnalyticsResponse>
    {
        private readonly AccelokaDbContext _context;

        public GetTicketSalesAnalyticsHandler(AccelokaDbContext context)
        {
            _context = context;
        }

        public async Task<TicketSalesAnalyticsResponse> Handle(GetTicketSalesAnalyticsQuery request, CancellationToken cancellationToken)
        {
            var to = request.To.HasValue
                ? DateTime.SpecifyKind(request.To.Value.Date, DateTimeKind.Utc)
                : DateTime.UtcNow.Date;
            var from = request.From.HasValue
                ? DateTime.SpecifyKind(request.From.Value.Date, DateTimeKind.Utc)
                : to.AddDays(-29);
            var toExclusive = to.AddDays(1);

            var filteredDetails = _context.BookedTicketDetails
                .Where(d => d.BookedTicket.BookingDate >= from && d.BookedTicket.BookingDate < toExclusive);

            var summaryRaw = await filteredDetails
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalOrders = g.Select(x => x.BookedTicketId).Distinct().Count(),
                    TotalTicketsSold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.Ticket.Price),
                    UniqueBuyers = g.Where(x => x.BookedTicket.UserId.HasValue)
                        .Select(x => x.BookedTicket.UserId)
                        .Distinct()
                        .Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            var topTickets = await filteredDetails
                .GroupBy(d => new { d.TicketId, d.Ticket.TicketCode, d.Ticket.TicketName })
                .Select(g => new TopSellingTicketDto
                {
                    TicketId = g.Key.TicketId,
                    TicketCode = g.Key.TicketCode,
                    TicketName = g.Key.TicketName,
                    SoldQuantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.Ticket.Price)
                })
                .OrderByDescending(x => x.SoldQuantity)
                .ThenBy(x => x.TicketCode)
                .Take(request.Top)
                .ToListAsync(cancellationToken);

            var trendRaw = await filteredDetails
                .Select(d => new
                {
                    d.BookedTicketId,
                    d.Quantity,
                    Revenue = d.Quantity * d.Ticket.Price,
                    BookingDate = d.BookedTicket.BookingDate
                })
                .ToListAsync(cancellationToken);

            List<SalesTrendPointDto> trend;
            if (request.GroupBy == "month")
            {
                trend = trendRaw
                    .GroupBy(x => new DateTime(x.BookingDate.Year, x.BookingDate.Month, 1, 0, 0, 0, DateTimeKind.Utc))
                    .OrderBy(g => g.Key)
                    .Select(g => new SalesTrendPointDto
                    {
                        PeriodStart = g.Key,
                        Orders = g.Select(x => x.BookedTicketId).Distinct().Count(),
                        TicketsSold = g.Sum(x => x.Quantity),
                        Revenue = g.Sum(x => x.Revenue)
                    })
                    .ToList();
            }
            else
            {
                trend = trendRaw
                    .GroupBy(x => x.BookingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new SalesTrendPointDto
                    {
                        PeriodStart = g.Key,
                        Orders = g.Select(x => x.BookedTicketId).Distinct().Count(),
                        TicketsSold = g.Sum(x => x.Quantity),
                        Revenue = g.Sum(x => x.Revenue)
                    })
                    .ToList();
            }

            return new TicketSalesAnalyticsResponse
            {
                From = from,
                To = to,
                GroupBy = request.GroupBy,
                Summary = new SalesSummaryDto
                {
                    TotalOrders = summaryRaw?.TotalOrders ?? 0,
                    TotalTicketsSold = summaryRaw?.TotalTicketsSold ?? 0,
                    TotalRevenue = summaryRaw?.TotalRevenue ?? 0,
                    UniqueBuyers = summaryRaw?.UniqueBuyers ?? 0
                },
                Trend = trend,
                TopTickets = topTickets
            };
        }
    }
}
