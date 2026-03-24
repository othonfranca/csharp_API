using FluentValidation;
using MinhaApi.DTOs;

namespace MinhaApi.Validators;

public class ProdutoGetValidator : AbstractValidator<ProdutoGetRequest>
{
    public ProdutoGetValidator()
    {
        RuleFor(x => x.Pagina)
            .GreaterThan(0)
            .WithMessage("Número da página deve ser maior que 0.");

        RuleFor(x => x.Tamanho)
            .InclusiveBetween(1, 50)
            .WithMessage("Tamanho da página deve ser entre 1 e 50.");

        // Regra para o Nome do Produto
        RuleFor(x => x.Nome)
            .MinimumLength(2)
            .When(x => !string.IsNullOrWhiteSpace(x.Nome))
            .WithMessage("O nome para busca deve ter pelo menos 2 caracteres.");

        // Regra para o Nome da Categoria
        RuleFor(x => x.CategoriaNome)
            .MinimumLength(3)
            .When(x => !string.IsNullOrWhiteSpace(x.CategoriaNome))
            .WithMessage("O nome da categoria deve ter pelo menos 3 caracteres.");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0)
            .When(x => x.CategoriaId.HasValue)
            .WithMessage("O ID da categoria para busca deve ser maior que 0.");
    }
}