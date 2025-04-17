using FluentValidation;
using quick_share.api.Business.Commands;

namespace quick_share.api.Business.Validations;

public class DeleteItemCommandValidator : AbstractValidator<DeleteItemCommand>
{
    public DeleteItemCommandValidator()
    {
        RuleFor(x => x.Session).NotNull();
        RuleFor(x => x.Session.Id).NotEmpty();
        RuleFor(x => x.ItemId).Must(y => y != Guid.Empty);
    }
}