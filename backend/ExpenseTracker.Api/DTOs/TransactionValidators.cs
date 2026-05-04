using FluentValidation;

namespace ExpenseTracker.Api.DTOs;

/// <summary>Shared validation rules for transaction create and update requests.</summary>
internal abstract class TransactionRequestValidatorBase<T> : AbstractValidator<T>
    where T : class
{
    protected void AddCommonRules(
        System.Linq.Expressions.Expression<Func<T, string>> typeExpr,
        System.Linq.Expressions.Expression<Func<T, decimal>> amountExpr,
        System.Linq.Expressions.Expression<Func<T, DateOnly>> dateExpr,
        System.Linq.Expressions.Expression<Func<T, Guid>> categoryIdExpr)
    {
        RuleFor(typeExpr)
            .NotEmpty().WithMessage("Type is required.")
            .Must(t => t == "income" || t == "expense")
            .WithMessage("Type must be 'income' or 'expense'.");

        RuleFor(amountExpr)
            .GreaterThan(0).WithMessage("Amount must be a positive number.");

        RuleFor(dateExpr)
            .NotEqual(default(DateOnly)).WithMessage("Date is required.");

        RuleFor(categoryIdExpr)
            .NotEqual(Guid.Empty).WithMessage("CategoryId is required.");
    }
}

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(t => t == "income" || t == "expense")
            .WithMessage("Type must be 'income' or 'expense'.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be a positive number.");

        RuleFor(x => x.Date)
            .NotEqual(default(DateOnly)).WithMessage("Date is required.");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).WithMessage("CategoryId is required.");
    }
}

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(t => t == "income" || t == "expense")
            .WithMessage("Type must be 'income' or 'expense'.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be a positive number.");

        RuleFor(x => x.Date)
            .NotEqual(default(DateOnly)).WithMessage("Date is required.");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).WithMessage("CategoryId is required.");
    }
}
