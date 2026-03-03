using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Features.Users.Dtos;
using Acceloka.Api.Infrastructure.Persistence;

namespace Acceloka.Api.Features.Users.Queries
{
    public class GetUserOrderHistoryQueryHandler : IRequestHandler<GetUserOrderHistoryQuery, OrderHistoryResponse>
    {
        private readonly AccelokaDbContext _dbContext;
        private readonly ILogger<GetUserOrderHistoryQueryHandler> _logger;

        public GetUserOrderHistoryQueryHandler(
            AccelokaDbContext dbContext,
            ILogger<GetUserOrderHistoryQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<OrderHistoryResponse> Handle(GetUserOrderHistoryQuery request, CancellationToken cancellationToken)
        {
            // Get all orders for user with pagination
            var query = _dbContext.BookedTickets
                .Where(bt => bt.UserId == request.UserId)
                .OrderByDescending(bt => bt.BookingDate);

            var totalItems = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            var bookedTickets = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(bt => bt.Details)
                .ThenInclude(btd => btd.Ticket)
                .ToListAsync(cancellationToken);

            var items = new List<OrderHistoryItemDto>();

            foreach (var bt in bookedTickets)
            {
                foreach (var detail in bt.Details)
                {
                    items.Add(new OrderHistoryItemDto
                    {
                        Id = detail.Id,
                        BookedTicketId = bt.Id,
                        TicketCode = detail.Ticket.TicketCode,
                        TicketName = detail.Ticket.TicketName,
                        Quantity = detail.Quantity,
                        Price = detail.Ticket.Price,
                        EventDate = detail.Ticket.EventDate,
                        BookedAt = bt.BookingDate
                    });
                }
            }

            _logger.LogInformation("Get order history for user: {UserId}, Page: {Page}, PageSize: {PageSize}", 
                request.UserId, request.Page, request.PageSize);

            return new OrderHistoryResponse
            {
                Items = items,
                Meta = new PaginationMeta
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }
    }
}
