using FluentValidation;
using quick_share.api.Business.Commands;

namespace quick_share.api.Business.Validations;

public class GetSessionCommandValidator : AbstractValidator<GetSessionCommand>
{
    public GetSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().Length(9, 10);
    }
}