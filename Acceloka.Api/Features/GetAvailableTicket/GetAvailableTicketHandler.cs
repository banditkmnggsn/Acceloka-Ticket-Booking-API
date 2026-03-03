using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Infrastructure.Persistence.Entities;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.GetAvailableTicket
{
    public class GetAvailableTicketHandler : IRequestHandler<GetAvailableTicketQuery, List<AvailableTicketResponse>>
    {
        private readonly AccelokaDbContext _context;
        private readonly ILogger<GetAvailableTicketHandler> _logger;

        public GetAvailableTicketHandler(AccelokaDbContext context, ILogger<GetAvailableTicketHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AvailableTicketResponse>> Handle(GetAvailableTicketQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetAvailableTicket: Processing request with filters");

            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.BookedTicketDetails)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.NamaKategori))
            {
                query = query.Where(t => t.Category.Name.Contains(request.NamaKategori));
            }

            if (!string.IsNullOrWhiteSpace(request.KodeTiket))
            {
                query = query.Where(t => t.TicketCode.Contains(request.KodeTiket));
            }

            if (!string.IsNullOrWhiteSpace(request.NamaTiket))
            {
                query = query.Where(t => t.TicketName.Contains(request.NamaTiket));
            }

            if (request.HargaMaksimal.HasValue)
            {
                query = query.Where(t => t.Price <= request.HargaMaksimal.Value);
            }

            if (request.TanggalEventMinimal.HasValue)
            {
                query = query.Where(t => t.EventDate >= request.TanggalEventMinimal.Value);
            }

            if (request.TanggalEventMaksimal.HasValue)
            {
                query = query.Where(t => t.EventDate <= request.TanggalEventMaksimal.Value);
            }

            // Apply sorting
            query = ApplyOrdering(query, request.OrderBy, request.OrderState);

            // Get total count for pagination info
            var totalCount = await query.CountAsync(cancellationToken);
            _logger.LogInformation("GetAvailableTicket: Found {Count} tickets matching criteria", totalCount);

            // Apply pagination
            var tickets = await query
                .Skip(request.GetSkip())
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Map to response
            var response = tickets.Select(ticket => new AvailableTicketResponse
            {
                NamaKategori = ticket.Category.Name,
                KodeTiket = ticket.TicketCode,
                NamaTiket = ticket.TicketName,
                ImageUrl = ticket.ImageUrl,
                Description = ticket.Description,
                TanggalEvent = ticket.EventDate,
                Harga = ticket.Price,
                SisaQuota = CalculateRemainingQuota(ticket)
            }).ToList();

            _logger.LogInformation("GetAvailableTicket: Returning {Count} tickets", response.Count);

            return response;
        }

        private IQueryable<Ticket> ApplyOrdering(
            IQueryable<Ticket> query,
            string orderBy,
            string orderState)
        {
            var isAscending = orderState.ToLower() == "asc";

            return orderBy.ToLower() switch
            {
                "kodetik" or "kodetiket" => isAscending
                    ? query.OrderBy(t => t.TicketCode)
                    : query.OrderByDescending(t => t.TicketCode),

                "nametik" or "nametikket" => isAscending
                    ? query.OrderBy(t => t.TicketName)
                    : query.OrderByDescending(t => t.TicketName),

                "harga" => isAscending
                    ? query.OrderBy(t => t.Price)
                    : query.OrderByDescending(t => t.Price),

                "tanggal" or "tanggalevent" => isAscending
                    ? query.OrderBy(t => t.EventDate)
                    : query.OrderByDescending(t => t.EventDate),

                _ => isAscending
                    ? query.OrderBy(t => t.TicketCode)
                    : query.OrderByDescending(t => t.TicketCode)
            };
        }

        private int CalculateRemainingQuota(Ticket ticket)
        {
            var bookedQuantity = ticket.BookedTicketDetails.Sum(btd => btd.Quantity);
            return Math.Max(0, ticket.Quota - bookedQuantity);
        }
    }
}
