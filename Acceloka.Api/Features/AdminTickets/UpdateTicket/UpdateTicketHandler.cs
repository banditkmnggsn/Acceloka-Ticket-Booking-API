using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Features.AdminTickets.CreateTicket;

namespace Acceloka.Api.Features.AdminTickets.UpdateTicket
{
    public class UpdateTicketHandler : IRequestHandler<UpdateTicketCommand, AdminTicketResponse>
    {
        private readonly AccelokaDbContext _context;

        public UpdateTicketHandler(AccelokaDbContext context)
        {
            _context = context;
        }

        public async Task<AdminTicketResponse> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TicketCode == request.CurrentTicketCode, cancellationToken);

            if (ticket == null)
            {
                throw new KeyNotFoundException($"Ticket dengan kode {request.CurrentTicketCode} tidak ditemukan");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category dengan ID {request.CategoryId} tidak ditemukan");
            }

            if (!string.Equals(request.CurrentTicketCode, request.TicketCode, StringComparison.OrdinalIgnoreCase))
            {
                var duplicateCode = await _context.Tickets
                    .AnyAsync(t => t.TicketCode == request.TicketCode && t.Id != ticket.Id, cancellationToken);

                if (duplicateCode)
                {
                    throw new InvalidOperationException($"TicketCode {request.TicketCode} sudah digunakan");
                }
            }

            var eventDateUtc = request.EventDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.EventDate, DateTimeKind.Utc)
                : request.EventDate.ToUniversalTime();

            ticket.TicketCode = request.TicketCode;
            ticket.TicketName = request.TicketName;
            ticket.CategoryId = request.CategoryId;
            ticket.EventDate = eventDateUtc;
            ticket.Price = request.Price;
            ticket.Quota = request.Quota;
            ticket.ImageUrl = request.ImageUrl;
            ticket.Description = request.Description;

            await _context.SaveChangesAsync(cancellationToken);

            return new AdminTicketResponse
            {
                Id = ticket.Id,
                TicketCode = ticket.TicketCode,
                TicketName = ticket.TicketName,
                CategoryId = ticket.CategoryId,
                CategoryName = category.Name,
                EventDate = ticket.EventDate,
                Price = ticket.Price,
                Quota = ticket.Quota,
                ImageUrl = ticket.ImageUrl,
                Description = ticket.Description
            };
        }
    }
}
