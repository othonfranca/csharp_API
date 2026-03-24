using FluentValidation;
using MinhaApi.DTOs;

namespace MinhaApi.Validators;

public class CategoriaUpdateValidator : AbstractValidator<CategoriaUpdateRequest>
{
    public CategoriaUpdateValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3);
    }
}