using System;
using FluentValidation;

namespace Lab4.Data;

public class CreateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
}

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("FullName is required and cannot exceed 100 characters.")
            .MaximumLength(100)
            .WithMessage("FullName is required and cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("A valid Email is required.")
            .EmailAddress()
            .WithMessage("A valid Email is required.");

        RuleFor(x => x.EnrollmentDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("EnrollmentDate cannot be in the future.");
    }
}

public class UpdateStudentRequest
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("A valid Id is required.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("FullName is required and cannot exceed 100 characters.")
            .MaximumLength(100)
            .WithMessage("FullName is required and cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("A valid Email is required.")
            .EmailAddress()
            .WithMessage("A valid Email is required.");
    }
}
