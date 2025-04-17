using FluentValidation;
using quick_share.api.Business.Commands;

namespace quick_share.api.Business.Validations;

public class GetBinaryItemCommandValidator : AbstractValidator<GetBinaryItemCommand>
{
    public GetBinaryItemCommandValidator()
    {
        RuleFor(x => x.Session).NotNull();
        RuleFor(x => x.Session.Id).NotEmpty();
        RuleFor(x => x.ItemId).Must(y => y != Guid.Empty);
    }
}