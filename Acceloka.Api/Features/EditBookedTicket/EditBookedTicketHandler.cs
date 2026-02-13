using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.EditBookedTicket
{
    public class EditBookedTicketHandler : IRequestHandler<EditBookedTicketCommand, EditBookedTicketResponse>
    {
        private readonly AccelokaDbContext _context;
        private readonly ILogger<EditBookedTicketHandler> _logger;

        public EditBookedTicketHandler(AccelokaDbContext context, ILogger<EditBookedTicketHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EditBookedTicketResponse> Handle(
            EditBookedTicketCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("EditBookedTicket: Processing edit for BookedTicketId {BookedTicketId}", request.BookedTicketId);

            // 1. VALIDASI: BookedTicketId harus ada
            var bookedTicket = await _context.BookedTickets
                .Include(bt => bt.Details)
                .ThenInclude(btd => btd.Ticket)
                .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(bt => bt.Id == request.BookedTicketId, cancellationToken);

            if (bookedTicket == null)
            {
                throw new KeyNotFoundException($"Booking dengan ID {request.BookedTicketId} tidak ditemukan");
            }

            // 2. VALIDASI untuk setiap ticket dalam request
            foreach (var reqTicket in request.Tickets)
            {
                // VALIDASI: KodeTiket harus ada di BookedTicket
                var detail = bookedTicket.Details.FirstOrDefault(d => d.Ticket.TicketCode == reqTicket.KodeTiket);

                if (detail == null)
                {
                    throw new InvalidOperationException(
                        $"Tiket dengan kode {reqTicket.KodeTiket} tidak ditemukan dalam booking ini");
                }

                // VALIDASI: Quantity minimal 1
                if (reqTicket.Quantity < 1)
                {
                    throw new InvalidOperationException(
                        $"Quantity untuk {reqTicket.KodeTiket} harus minimal 1");
                }

                // VALIDASI: Quantity <= sisa quota
                var ticket = detail.Ticket;
                int totalBooked = ticket.BookedTicketDetails.Sum(btd => btd.Quantity);
                int oldQuantity = detail.Quantity;
                int newQuantity = reqTicket.Quantity;
                int quantityDifference = newQuantity - oldQuantity;

                int availableQuota = ticket.Quota - (totalBooked - oldQuantity);

                if (newQuantity > ticket.Quota)
                {
                    throw new InvalidOperationException(
                        $"Quantity untuk {reqTicket.KodeTiket} melebihi total quota ({ticket.Quota})");
                }

                if (quantityDifference > availableQuota)
                {
                    throw new InvalidOperationException(
                        $"Permintaan untuk {reqTicket.KodeTiket} melebihi sisa quota tersedia");
                }
            }

            // 3. UPDATE quantities
            var firstTicket = request.Tickets.First();
            var firstDetail = bookedTicket.Details.First(d => d.Ticket.TicketCode == firstTicket.KodeTiket);

            foreach (var reqTicket in request.Tickets)
            {
                var detail = bookedTicket.Details.First(d => d.Ticket.TicketCode == reqTicket.KodeTiket);
                detail.Quantity = reqTicket.Quantity;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("EditBookedTicket: Successfully updated booking");

            // 4. RETURN Response (dari ticket pertama)
            return new EditBookedTicketResponse
            {
                KodeTiket = firstDetail.Ticket.TicketCode,
                NamaTiket = firstDetail.Ticket.TicketName,
                NamaKategori = firstDetail.Ticket.Category.Name,
                SisaQuantity = firstDetail.Quantity
            };
        }
    }
}
