using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Resumes.Application.Features.Persons.UpdatePerson;

public class UpdatePersonValidator : Validator<UpdatePersonRequest>
{
    public UpdatePersonValidator()
    {
        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("'JobTitle' must not exceed 100 characters.");

        RuleFor(x => x.Firstname)
            .MaximumLength(100).WithMessage("'Firstname' must not exceed 100 characters.");

        RuleFor(x => x.Lastname)
            .MaximumLength(100).WithMessage("'Lastname' must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("'Email' must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Email)
            .MaximumLength(100).WithMessage("'Email' must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(100).WithMessage("'Phone' must not exceed 100 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("'Country' must not exceed 100 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("'City' must not exceed 100 characters.");

        RuleFor(x => x.Street)
            .MaximumLength(100).WithMessage("'Street' must not exceed 100 characters.");

        RuleFor(x => x.PostCode)
            .MaximumLength(100).WithMessage("'PostCode' must not exceed 100 characters.");

        RuleFor(x => x.Nationality)
            .MaximumLength(100).WithMessage("'Nationality' must not exceed 100 characters.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("'DateOfBirth' must not be in the future.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.DrivingLicense)
            .MaximumLength(100).WithMessage("'DrivingLicense' must not exceed 100 characters.");
    }
}
