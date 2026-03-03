using FluentValidation;
using Acceloka.Api.Features.Auth.Queries;

namespace Acceloka.Api.Features.Auth.Validators
{
    public class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
    {
        public RefreshTokenQueryValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required");
        }
    }
}
