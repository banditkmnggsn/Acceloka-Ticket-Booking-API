using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.GetBookedTicket
{
    public class GetBookedTicketHandler : IRequestHandler<GetBookedTicketQuery, GetBookedTicketGroupedResponse>
    {
        private readonly AccelokaDbContext _context;
        private readonly ILogger<GetBookedTicketHandler> _logger;

        public GetBookedTicketHandler(AccelokaDbContext context, ILogger<GetBookedTicketHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GetBookedTicketGroupedResponse> Handle(
            GetBookedTicketQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetBookedTicket: Fetching booking {BookedTicketId}", request.BookedTicketId);

            // VALIDASI: BookedTicketId harus ada
            var bookedTicket = await _context.BookedTickets
                .Include(bt => bt.Details)
                .ThenInclude(btd => btd.Ticket)
                .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(bt => bt.Id == request.BookedTicketId, cancellationToken);

            if (bookedTicket == null)
            {
                throw new KeyNotFoundException($"Booking dengan ID {request.BookedTicketId} tidak ditemukan");
            }

            // GROUP BY kategori
            var groupedByCategory = bookedTicket.Details
                .GroupBy(d => d.Ticket.Category.Name)
                .Select(g => new BookedTicketCategoryGroupDto
                {
                    NamaKategori = g.Key,
                    Items = g.Select(d => new BookedTicketItemDto
                    {
                        KodeTiket = d.Ticket.TicketCode,
                        NamaTiket = d.Ticket.TicketName,
                        TanggalEvent = d.Ticket.EventDate,
                        Quantity = d.Quantity
                    })
                    .ToList()
                })
                .ToList();

            var response = new GetBookedTicketGroupedResponse
            {
                BookedTicketId = bookedTicket.Id,
                Categories = groupedByCategory
            };

            _logger.LogInformation("GetBookedTicket: Found {CategoryCount} categories with {ItemCount} total items", 
                groupedByCategory.Count, 
                bookedTicket.Details.Count);

            return response;
        }
    }
}
