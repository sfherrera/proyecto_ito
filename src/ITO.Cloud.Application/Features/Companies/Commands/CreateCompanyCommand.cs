using FluentValidation;
using ITO.Cloud.Application.Features.Companies.DTOs;
using MediatR;

namespace ITO.Cloud.Application.Features.Companies.Commands;

public record CreateCompanyCommand(
    string Name, string? Rut, string? BusinessName,
    string CompanyType, string? Address, string? City,
    string? Region, string? Phone, string? Email,
    string? Website, string? Notes) : IRequest<CompanyDto>;

public record UpdateCompanyCommand(
    Guid Id, string Name, string? Rut, string? BusinessName,
    string CompanyType, string? Address, string? City,
    string? Region, string? Phone, string? Email,
    string? Website, bool IsActive, string? Notes) : IRequest<CompanyDto>;

public record DeleteCompanyCommand(Guid Id) : IRequest;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Nombre es requerido (máx 200 caracteres).");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Email inválido.");
        RuleFor(x => x.CompanyType).Must(t => new[] { "constructora", "inmobiliaria", "mixta", "consultora", "otra" }.Contains(t))
            .WithMessage("Tipo de empresa inválido.");
    }
}

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}
