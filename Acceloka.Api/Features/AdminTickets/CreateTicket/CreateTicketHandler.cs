using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Infrastructure.Persistence.Entities;

namespace Acceloka.Api.Features.AdminTickets.CreateTicket
{
    public class CreateTicketHandler : IRequestHandler<CreateTicketCommand, AdminTicketResponse>
    {
        private readonly AccelokaDbContext _context;

        public CreateTicketHandler(AccelokaDbContext context)
        {
            _context = context;
        }

        public async Task<AdminTicketResponse> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category dengan ID {request.CategoryId} tidak ditemukan");
            }

            var duplicateCode = await _context.Tickets
                .AnyAsync(t => t.TicketCode == request.TicketCode, cancellationToken);

            if (duplicateCode)
            {
                throw new InvalidOperationException($"TicketCode {request.TicketCode} sudah digunakan");
            }

            var eventDateUtc = request.EventDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.EventDate, DateTimeKind.Utc)
                : request.EventDate.ToUniversalTime();

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                TicketCode = request.TicketCode,
                TicketName = request.TicketName,
                CategoryId = request.CategoryId,
                EventDate = eventDateUtc,
                Price = request.Price,
                Quota = request.Quota,
                ImageUrl = request.ImageUrl,
                Description = request.Description
            };

            _context.Tickets.Add(ticket);
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
