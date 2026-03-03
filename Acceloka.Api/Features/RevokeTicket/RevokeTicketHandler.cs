using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.RevokeTicket
{
    public class RevokeTicketHandler : IRequestHandler<RevokeTicketCommand, RevokeTicketResponse>
    {
        private readonly AccelokaDbContext _context;
        private readonly ILogger<RevokeTicketHandler> _logger;

        public RevokeTicketHandler(AccelokaDbContext context, ILogger<RevokeTicketHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RevokeTicketResponse> Handle(
            RevokeTicketCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("RevokeTicket: Processing revoke for BookedTicketId {BookedTicketId}", request.BookedTicketId);

            // CRITICAL: Gunakan transaction dengan explicit locking untuk mencegah race condition
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
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

                // VALIDASI: User hanya bisa revoke booking milik mereka
                if (request.UserId.HasValue && bookedTicket.UserId != request.UserId)
                {
                    throw new UnauthorizedAccessException("Anda tidak memiliki akses untuk membatalkan booking ini");
                }

                // 2. VALIDASI: KodeTiket harus ada di BookedTicket
                var detail = bookedTicket.Details.FirstOrDefault(d => d.Ticket.TicketCode == request.KodeTiket);

                if (detail == null)
                {
                    throw new InvalidOperationException(
                        $"Tiket dengan kode {request.KodeTiket} tidak ditemukan dalam booking ini");
                }

                // Lock ticket untuk mencegah concurrent modification
                var ticketIds = new[] { detail.TicketId };
                var lockedTickets = await _context.Tickets
                    .FromSqlRaw(@"
                        SELECT t.* 
                        FROM ""Tickets"" t
                        WHERE t.""Id"" = ANY({0})
                        FOR UPDATE", 
                        ticketIds)
                    .Include(t => t.Category)
                    .ToListAsync(cancellationToken);

                // 3. VALIDASI: Qty <= qty yang sudah dibooking (SETELAH LOCK)
                if (request.Qty <= 0)
                {
                    throw new InvalidOperationException("Qty harus lebih dari 0");
                }

                if (request.Qty > detail.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Qty yang di-revoke ({request.Qty}) melebihi quantity yang di-booking ({detail.Quantity})");
                }

                // 4. PROSES REVOKE
                var ticketInfo = lockedTickets.First();
                int sisaQuantity = detail.Quantity - request.Qty;
                detail.Quantity = sisaQuantity;

                // Jika quantity menjadi 0, hapus detail
                if (detail.Quantity == 0)
                {
                    _context.BookedTicketDetails.Remove(detail);
                }

                // Cek apakah semua detail sudah 0, jika ya hapus booking
                if (!bookedTicket.Details.Any(d => d.Quantity > 0 && d.Id != detail.Id))
                {
                    _context.BookedTickets.Remove(bookedTicket);
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Commit transaction setelah semua berhasil
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("RevokeTicket: Successfully revoked {Qty} tickets", request.Qty);

                // 5. RETURN Response
                return new RevokeTicketResponse
                {
                    KodeTiket = ticketInfo.TicketCode,
                    NamaTiket = ticketInfo.TicketName,
                    NamaKategori = ticketInfo.Category.Name,
                    SisaQuantity = sisaQuantity
                };
            }
            catch
            {
                // Rollback jika ada error
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
