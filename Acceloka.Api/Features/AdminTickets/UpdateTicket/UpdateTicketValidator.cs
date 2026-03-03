using FluentValidation;

namespace Acceloka.Api.Features.AdminTickets.UpdateTicket
{
    public class UpdateTicketValidator : AbstractValidator<UpdateTicketCommand>
    {
        public UpdateTicketValidator()
        {
            RuleFor(x => x.CurrentTicketCode)
                .NotEmpty().WithMessage("CurrentTicketCode is required");

            RuleFor(x => x.TicketCode)
                .NotEmpty().WithMessage("TicketCode is required");

            RuleFor(x => x.TicketName)
                .NotEmpty().WithMessage("TicketName is required");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required");

            RuleFor(x => x.EventDate)
                .GreaterThan(DateTime.UtcNow.Date)
                .WithMessage("EventDate must be in the future");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.Quota)
                .GreaterThan(0).WithMessage("Quota must be greater than 0");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidAbsoluteHttpUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("ImageUrl must be a valid http/https URL");
        }

        private static bool BeValidAbsoluteHttpUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return true;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}
