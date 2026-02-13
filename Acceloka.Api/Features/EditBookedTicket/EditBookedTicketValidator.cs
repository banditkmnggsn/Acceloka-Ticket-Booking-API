using FluentValidation;

namespace Acceloka.Api.Features.EditBookedTicket
{
    public class EditBookedTicketValidator : AbstractValidator<EditBookedTicketCommand>
    {
        public EditBookedTicketValidator()
        {
            RuleFor(x => x.BookedTicketId)
                .NotEmpty()
                .WithMessage("BookedTicketId tidak boleh kosong");

            RuleFor(x => x.Tickets)
                .NotEmpty()
                .WithMessage("Minimal harus ada 1 tiket untuk di-edit");

            RuleForEach(x => x.Tickets)
                .ChildRules(ticket =>
                {
                    ticket.RuleFor(t => t.KodeTiket)
                        .NotEmpty()
                        .WithMessage("Kode Tiket tidak boleh kosong");

                    ticket.RuleFor(t => t.Quantity)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage("Quantity harus minimal 1");
                });
        }
    }
}
