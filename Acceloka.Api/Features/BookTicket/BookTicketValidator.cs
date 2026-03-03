using FluentValidation;

namespace Acceloka.Api.Features.BookTicket
{
    public class BookTicketValidator : AbstractValidator<BookTicketCommand>
    {
        public BookTicketValidator()
        {
            RuleFor(x => x.Tickets)
                .NotEmpty()
                .WithMessage("Minimal harus ada 1 tiket untuk di-booking");

            RuleForEach(x => x.Tickets)
                .ChildRules(ticket =>
                {
                    ticket.RuleFor(t => t.KodeTiket)
                        .NotEmpty()
                        .WithMessage("Kode Tiket tidak boleh kosong")
                        .MinimumLength(1)
                        .WithMessage("Kode Tiket tidak valid");

                    ticket.RuleFor(t => t.Quantity)
                        .GreaterThan(0)
                        .WithMessage("Quantity harus lebih dari 0");
                });
        }
    }
}
