using FluentValidation;
using quick_share.api.Business.Commands;

namespace quick_share.api.Business.Validations;

public class AddSimpleItemCommandValidator : AbstractValidator<AddSimpleItemCommand>
{
    public AddSimpleItemCommandValidator()
    {
        RuleFor(x => x.Session).NotNull();
        RuleFor(x => x.Session.Id).NotEmpty();
        RuleFor(x => x.ItemValue).NotEmpty();
    }
}