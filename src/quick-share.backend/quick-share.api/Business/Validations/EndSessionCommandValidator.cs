using FluentValidation;
using quick_share.api.Business.Commands;

namespace quick_share.api.Business.Validations;

public class EndSessionCommandValidator : AbstractValidator<EndSessionCommand>
{
    public EndSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().Length(9, 10);
    }
}