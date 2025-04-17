using FluentValidation;
using quick_share.api.Business.Commands;

namespace quick_share.api.Business.Validations;

public class AddBinaryItemCommandValidator : AbstractValidator<AddBinaryItemCommand>
{
    public AddBinaryItemCommandValidator()
    {
        RuleFor(x => x.Session).NotNull();
        RuleFor(x => x.Session.Id).NotEmpty();
        RuleFor(x => x.FormFile).NotNull();
    }
}