using FluentValidation;

namespace Acceloka.Api.Features.GetAvailableTicket
{
    public class GetAvailableTicketValidator : AbstractValidator<GetAvailableTicketQuery>
    {
        public GetAvailableTicketValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page harus minimal 1");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("PageSize harus minimal 1")
                .LessThanOrEqualTo(100)
                .WithMessage("PageSize maksimal 100");

            RuleFor(x => x.HargaMaksimal)
                .GreaterThan(0)
                .When(x => x.HargaMaksimal.HasValue)
                .WithMessage("Harga Maksimal harus lebih dari 0");

            RuleFor(x => x.TanggalEventMinimal)
                .LessThanOrEqualTo(x => x.TanggalEventMaksimal)
                .When(x => x.TanggalEventMinimal.HasValue && x.TanggalEventMaksimal.HasValue)
                .WithMessage("Tanggal Event Minimal harus lebih kecil atau sama dengan Tanggal Event Maksimal");

            RuleFor(x => x.OrderState)
                .Must(x => x.ToLower() == "asc" || x.ToLower() == "desc")
                .WithMessage("OrderState harus 'asc' atau 'desc'");
        }
    }
}
