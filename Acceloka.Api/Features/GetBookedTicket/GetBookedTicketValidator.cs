using FluentValidation;

namespace Acceloka.Api.Features.GetBookedTicket
{
    public class GetBookedTicketValidator : AbstractValidator<GetBookedTicketQuery>
    {
        public GetBookedTicketValidator()
        {
            RuleFor(x => x.BookedTicketId)
                .NotEmpty()
                .WithMessage("BookedTicketId tidak boleh kosong");
        }
    }
}
