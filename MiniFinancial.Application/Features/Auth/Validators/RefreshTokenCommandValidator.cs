using FluentValidation;
using MiniFinancial.Application.Features.Auth.Commands;

namespace MiniFinancial.Application.Features.Auth.Validators
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                    .NotEmpty().WithMessage("Refresh Token informado invalido.")
                    .MaximumLength(500).WithMessage("Refresh Token informado ultrapassa o limite.");
        }
    }
}
