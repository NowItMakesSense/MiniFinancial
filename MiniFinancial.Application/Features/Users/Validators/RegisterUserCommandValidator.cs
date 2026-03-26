using FluentValidation;
using MiniFinancial.Application.Features.Users.Commands;
using MiniFinancial.Domain.Enums;

namespace MiniFinancial.Application.Features.Users.Validators
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(150).WithMessage("O nome não pode exceder 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O email é obrigatório.")
                .EmailAddress().WithMessage("Formato de email inválido.")
                .MaximumLength(200).WithMessage("O email não pode exceder 200 caracteres.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("A funcao do usuario deve ser informada.")
                .IsInEnum().WithMessage("Role inválida.");
        }
    }
}
