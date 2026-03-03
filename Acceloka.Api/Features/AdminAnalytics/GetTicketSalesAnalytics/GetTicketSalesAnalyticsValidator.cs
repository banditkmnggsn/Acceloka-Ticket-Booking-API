using FluentValidation;

namespace Acceloka.Api.Features.AdminAnalytics.GetTicketSalesAnalytics
{
    public class GetTicketSalesAnalyticsValidator : AbstractValidator<GetTicketSalesAnalyticsQuery>
    {
        public GetTicketSalesAnalyticsValidator()
        {
            RuleFor(x => x.GroupBy)
                .Must(x => x == "day" || x == "month")
                .WithMessage("GroupBy must be 'day' or 'month'");

            RuleFor(x => x.Top)
                .InclusiveBetween(1, 50)
                .WithMessage("Top must be between 1 and 50");

            RuleFor(x => x)
                .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value.Date <= x.To.Value.Date)
                .WithMessage("From must be less than or equal to To");
        }
    }
}
