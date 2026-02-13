using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Infrastructure.Persistence.Entities;
using Acceloka.Api.Features.Common.Dtos;
using Acceloka.Api.Infrastructure.Middleware;

namespace Acceloka.Api.Features.BookTicket
{
    public class BookTicketHandler : IRequestHandler<BookTicketCommand, BookedTicketResponse>
    {
        private readonly AccelokaDbContext _context;
        private readonly ILogger<BookTicketHandler> _logger;

        public BookTicketHandler(AccelokaDbContext context, ILogger<BookTicketHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BookedTicketResponse> Handle(
            BookTicketCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("BookTicket: Processing booking request");

            // 1. VALIDASI: Cek semua tiket ada di database
            var ticketCodes = request.Tickets.Select(t => t.KodeTiket).ToList();
            var tickets = await _context.Tickets
                .Include(t => t.BookedTicketDetails)
                .Include(t => t.Category)
                .Where(t => ticketCodes.Contains(t.TicketCode))
                .ToListAsync(cancellationToken);

            if (tickets.Count != request.Tickets.Count)
            {
                throw new InvalidOperationException("Salah satu atau lebih kode tiket tidak ditemukan");
            }

            // 2. VALIDASI: Cek quota & tanggal event
            foreach (var reqTicket in request.Tickets)
            {
                var ticket = tickets.First(t => t.TicketCode == reqTicket.KodeTiket);

                // Cek tanggal event tidak boleh <= tanggal booking (hari ini)
                if (ticket.EventDate.Date <= DateTime.Now.Date)
                {
                    throw new InvalidOperationException(
                        $"Tiket {ticket.TicketCode} sudah tidak tersedia (tanggal event sudah lewat)");
                }

                // Hitung sisa quota
                int bookedQuantity = ticket.BookedTicketDetails.Sum(btd => btd.Quantity);
                int remainingQuota = ticket.Quota - bookedQuantity;

                // Cek quota tidak habis
                if (remainingQuota <= 0)
                {
                    throw new InvalidOperationException(
                        $"Tiket {ticket.TicketCode} sudah habis");
                }

                // Cek quantity tidak melebihi sisa quota
                if (reqTicket.Quantity > remainingQuota)
                {
                    throw new InvalidOperationException(
                        $"Permintaan tiket {ticket.TicketCode} melebihi sisa quota ({remainingQuota} tersisa)");
                }

                if (reqTicket.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Quantity untuk {ticket.TicketCode} harus lebih dari 0");
                }
            }

            // 3. SIMPAN ke database
            var bookedTicket = new BookedTicket
            {
                Id = Guid.NewGuid(),
                BookingDate = DateTime.Now,
                Details = new List<BookedTicketDetail>()
            };

            var totalByCategory = new Dictionary<string, decimal>();
            decimal grandTotal = 0;

            foreach (var reqTicket in request.Tickets)
            {
                var ticket = tickets.First(t => t.TicketCode == reqTicket.KodeTiket);

                var detail = new BookedTicketDetail
                {
                    Id = Guid.NewGuid(),
                    BookedTicketId = bookedTicket.Id,
                    TicketId = ticket.Id,
                    Quantity = reqTicket.Quantity
                };

                bookedTicket.Details.Add(detail);

                // Hitung total per kategori & grand total
                decimal subtotal = ticket.Price * reqTicket.Quantity;
                string categoryName = ticket.Category.Name;

                if (totalByCategory.ContainsKey(categoryName))
                {
                    totalByCategory[categoryName] += subtotal;
                }
                else
                {
                    totalByCategory[categoryName] = subtotal;
                }

                grandTotal += subtotal;
            }

            _context.BookedTickets.Add(bookedTicket);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("BookTicket: Booking created with ID {BookingId}", bookedTicket.Id);

            // 4. RETURN Response
            return new BookedTicketResponse
            {
                NamaTiket = tickets.First().TicketName,
                KodeTiket = request.Tickets.First().KodeTiket,
                Harga = tickets.First().Price,
                TotalPerKategori = totalByCategory,
                TotalSemua = grandTotal
            };
        }
    }
}
