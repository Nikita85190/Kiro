using FluentValidation;

namespace ExpenseTracker.Api.DTOs;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Category name cannot be whitespace only.");
    }
}

public class RenameCategoryRequestValidator : AbstractValidator<RenameCategoryRequest>
{
    public RenameCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Category name cannot be whitespace only.");
    }
}