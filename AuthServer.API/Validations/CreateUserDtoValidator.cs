using AuthServer.Core.Dtos;
using FluentValidation;

namespace AuthServer.API.Validations
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").NotNull().WithMessage("Email is required.").EmailAddress().WithMessage("Email is wrong");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.").NotNull().WithMessage("Password is required.");
            RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required.").NotNull().WithMessage("UserName is required.");
        }
    }
}
