using EMI.EmployeeManagement.BLL.Validators;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;

namespace EMI.EmployeeManagement.Tests.BLL.Validators;

public class UpdateEmployeeValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsSuccess()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(1);

        var result = UpdateEmployeeValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidId_ReturnsInvalidIdError()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(0);

        var result = UpdateEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == EmployeeValidationErrorEnum.InvalidId);
    }

    [Fact]
    public void Validate_NameExceeds100Chars_ReturnsInvalidNameError()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(1);
        request.Name = new string('A', 101);

        var result = UpdateEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == EmployeeValidationErrorEnum.InvalidName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ReturnsSuccess(string? name)
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(1);
        request.Name = name;

        var result = UpdateEmployeeValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidSalary_ReturnsInvalidSalaryError()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(1);
        request.Salary = -1;

        var result = UpdateEmployeeValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == EmployeeValidationErrorEnum.InvalidSalary);
    }

    [Fact]
    public void Validate_NullSalary_ReturnsSuccess()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(1);
        request.Salary = null;

        var result = UpdateEmployeeValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
