using FluentValidation;

namespace Acceloka.Api.Features.RevokeTicket
{
    public class RevokeTicketValidator : AbstractValidator<RevokeTicketCommand>
    {
        public RevokeTicketValidator()
        {
            RuleFor(x => x.BookedTicketId)
                .NotEmpty()
                .WithMessage("BookedTicketId tidak boleh kosong");

            RuleFor(x => x.KodeTiket)
                .NotEmpty()
                .WithMessage("Kode Tiket tidak boleh kosong");

            RuleFor(x => x.Qty)
                .GreaterThan(0)
                .WithMessage("Qty harus lebih dari 0");
        }
    }
}
