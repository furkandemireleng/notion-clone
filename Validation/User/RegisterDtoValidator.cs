using notion_clone.Dto;
using FluentValidation;

namespace notion_clone.Validatior.User;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(r => r.Username)
            .NotEmpty().WithMessage("invalid_email")
            .EmailAddress().WithMessage("invalid_email");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("invalid_password")
            .MinimumLength(6).WithMessage("invalid_password") // at least 6 lenght
            .Matches("(?=.*[a-z])") // At least one lowercase
            .WithMessage("invalid_password")
            .Matches("(?=.*[A-Z])") // At least one uppercase
            .WithMessage("invalid_password")
            .Matches("(?=.*[0-9])") // At least one digit
            .WithMessage("invalid_password")
            .Matches("^(?=.*[^a-zA-Z0-9\\s]).+$") // At least one special character
            .WithMessage("invalid_password");
    }
}