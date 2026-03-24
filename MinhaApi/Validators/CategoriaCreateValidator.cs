using FluentValidation;
using MinhaApi.DTOs;

namespace MinhaApi.Validators;

public class CategoriaCreateValidator : AbstractValidator<CategoriaCreateRequest>
{
    public CategoriaCreateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da categoria é obrigatório.")
            .MinimumLength(3).WithMessage("Nome da categoria deve ter pelo menos 3 caracteres.")
            .MaximumLength(50).WithMessage("Nome da categoria deve ter no máximo 50 caracteres.");
    }
}