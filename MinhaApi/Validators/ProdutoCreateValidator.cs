using FluentValidation;
using MinhaApi.DTOs;

namespace MinhaApi.Validators;

public class ProdutoCreateValidator : AbstractValidator<ProdutoCreateRequest>
{
    public ProdutoCreateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome do produto é obrigatório.")
            .MinimumLength(3)
            .WithMessage("Nome do produto deve ter pelo menos 3 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0)
            .WithMessage("Preço deve ser maior que 0.");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0)
            .WithMessage("Categoria deve ser maior que 0.");

    }
}