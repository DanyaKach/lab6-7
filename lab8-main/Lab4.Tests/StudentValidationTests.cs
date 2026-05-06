using FluentValidation.TestHelper;
using Lab4.Data;
using Xunit;

namespace Lab4.Tests;

public class StudentValidationTests
{
    private readonly CreateStudentRequestValidator _createValidator = new();
    private readonly UpdateStudentRequestValidator _updateValidator = new();

    [Fact]
    public void CreateStudent_ValidRequest_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            FullName = "John Doe",
            Email = "johndoe@example.com",
            EnrollmentDate = DateTime.UtcNow.AddDays(-1) // Past date
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateStudent_EmptyFullName_ShouldHaveValidationErrorForFullName()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            FullName = "",
            Email = "johndoe@example.com",
            EnrollmentDate = DateTime.UtcNow
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("FullName is required and cannot exceed 100 characters.");

        // Verify it doesn't fail for email
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CreateStudent_InvalidEmail_ShouldHaveValidationErrorForEmail()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            FullName = "John Doe",
            Email = "invalid-email-format",
            EnrollmentDate = DateTime.UtcNow
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("A valid Email is required.");
    }

    [Fact]
    public void CreateStudent_FutureEnrollmentDate_ShouldHaveValidationErrorForEnrollmentDate()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            FullName = "John Doe",
            Email = "johndoe@example.com",
            EnrollmentDate = DateTime.UtcNow.AddDays(1) // Future date!
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EnrollmentDate)
            .WithErrorMessage("EnrollmentDate cannot be in the future.");
    }

    [Fact]
    public void UpdateStudent_ZeroId_ShouldHaveValidationErrorForId()
    {
        // Arrange
        var request = new UpdateStudentRequest
        {
            Id = 0, // Invalid ID
            FullName = "Jane Doe",
            Email = "jane@example.com"
        };

        // Act
        var result = _updateValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("A valid Id is required.");
    }

    [Fact]
    public void UpdateStudent_EmptyEmail_ShouldHaveValidationErrorForEmail()
    {
        // Arrange
        var request = new UpdateStudentRequest
        {
            Id = 1,
            FullName = "Jane Doe",
            Email = "" // Empty email
        };

        // Act
        var result = _updateValidator.TestValidate(request);

        // Assert
        // We expect it to fail the NotEmpty and EmailAddress rules
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("A valid Email is required.");
    }
}