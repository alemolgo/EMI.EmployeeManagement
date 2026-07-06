using EMI.EmployeeManagement.BLL.Validators;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;

namespace EMI.EmployeeManagement.Tests.BLL.Validators;

public class NewEmployeeValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsSuccess()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();

        var result = NewEmployeeValidator.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ReturnsInvalidNameError(string? name)
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        request.Name = name;

        var result = NewEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Code == EmployeeValidationErrorEnum.InvalidName &&
            e.Message.Contains("required"));
    }

    [Fact]
    public void Validate_NameExceeds100Chars_ReturnsInvalidNameError()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        request.Name = new string('A', 101);

        var result = NewEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Code == EmployeeValidationErrorEnum.InvalidName &&
            e.Message.Contains("100 characters"));
    }

    [Fact]
    public void Validate_InvalidPositionId_ReturnsInvalidCurrentPositionIdError()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        request.CurrentPositionId = 0;

        var result = NewEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Code == EmployeeValidationErrorEnum.InvalidCurrentPositionId);
    }

    [Fact]
    public void Validate_InvalidSalary_ReturnsInvalidSalaryError()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        request.Salary = 0;

        var result = NewEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Code == EmployeeValidationErrorEnum.InvalidSalary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyPassword_ReturnsInvalidPasswordError(string? password)
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        request.Password = password;

        var result = NewEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Code == EmployeeValidationErrorEnum.InvalidPassword);
    }

    [Fact]
    public void Validate_PasswordExceeds8Chars_ReturnsError()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        request.Password = new string('A', 9);

        var result = NewEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Code == EmployeeValidationErrorEnum.InvalidName &&
            e.Message.Contains("8 characters"));
    }

    [Fact]
    public void Validate_MultipleInvalidFields_ReturnsMultipleErrors()
    {
        var request = new CreateEmployeeRequest
        {
            Name = "",
            CurrentPositionId = 0,
            Salary = 0,
            Password = ""
        };

        var result = NewEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(2);
    }
}
