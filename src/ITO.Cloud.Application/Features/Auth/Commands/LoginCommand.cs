using FluentValidation;
using ITO.Cloud.Application.Features.Auth.DTOs;
using MediatR;

namespace ITO.Cloud.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email inválido.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Contraseña requerida.");
    }
}
