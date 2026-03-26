using FluentValidation;
using MiniFinancial.Application.Features.Users.Commands;

namespace MiniFinancial.Application.Features.Users.Validators
{
    public class RemoveUserCommandValidator : AbstractValidator<RemoveUserCommand>
    {
        public RemoveUserCommandValidator() 
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id do Usuario deve ser informado.");
        }
    }
}
