using FluentValidation;
using ModelBaseAPI.Models.Request;

namespace ModelBaseAPI.Models.Validator
{
    public class EmployeeValidator : AbstractValidator<EmployeeRequest>
    {
        public EmployeeValidator() 
        {
            RuleFor(user => user.Name)
            .NotEmpty().WithMessage("The name can't be null.");

            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("E-mail is required.")
                .EmailAddress().WithMessage("E-mail invalid.");

            RuleFor(user => user.Age)
                .GreaterThan(18).WithMessage("The age must be over 18 years old.");

        }
    }
}
