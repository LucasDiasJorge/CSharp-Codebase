using System;
using FluentValidation;
using FluentValidationUserApi.Models;

namespace FluentValidationUserApi.Validators
{
    /// <summary>
    /// Provides validation rules for the <see cref="User"/> entity using FluentValidation.
    /// </summary>
    public class UserValidator : AbstractValidator<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserValidator"/> class.
        /// </summary>
        public UserValidator()
        {
            RuleFor(user => user.Name)
                .NotEmpty().WithMessage("Name is required. Please provide the full name.")
                .MinimumLength(2).WithMessage("Name must contain at least 2 characters.")
                .MaximumLength(100).WithMessage("Name can contain a maximum of 100 characters.");

            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email is required. Please provide a contact email address.")
                .EmailAddress().WithMessage("Email must be a valid email address (e.g., user@example.com).");

            RuleFor(user => user.Age)
                .InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120 years old.")
                .Must((user, age) => AgeMatchesDateOfBirth(user.DateOfBirth, age))
                .WithMessage("Age must match the calculated value based on Date of Birth.");

            RuleFor(user => user.DateOfBirth)
                .NotEmpty().WithMessage("Date of Birth is required. Provide the date in ISO 8601 format (yyyy-MM-dd).")
                .LessThan(DateTime.Today).WithMessage("Date of Birth must be in the past.")
                .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of Birth indicates an age greater than 120 years, which is not allowed.");
        }

        private static bool AgeMatchesDateOfBirth(DateTime dateOfBirth, int age)
        {
            if (dateOfBirth == DateTime.MinValue)
            {
                return false;
            }

            DateTime today = DateTime.Today;
            int calculatedAge = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-calculatedAge))
            {
                calculatedAge--;
            }

            return calculatedAge == age;
        }
    }
}
