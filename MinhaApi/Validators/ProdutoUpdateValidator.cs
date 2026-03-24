using FluentValidation;
using MinhaApi.DTOs;

namespace MinhaApi.Validators;

public class ProdutoUpdateValidator : AbstractValidator<ProdutoUpdateRequest>
{
    public ProdutoUpdateValidator()
    {
        RuleFor(x => x.Preco)
            .GreaterThan(0)
            .WithMessage("O preço deve ser maior que zero.");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0)
            .WithMessage("O ID da categoria deve ser válido.");
    }
}