using EMI.EmployeeManagement.BLL.Validators;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;

namespace EMI.EmployeeManagement.Tests.BLL.Validators;

public class UpdatePositionValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsSuccess()
    {
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(1, 2);

        var result = UpdatePositionValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidEmployeeId_ReturnsInvalidIdError()
    {
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(0, 2);

        var result = UpdatePositionValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == EmployeeValidationErrorEnum.InvalidId);
    }

    [Fact]
    public void Validate_InvalidNewPositionId_ReturnsInvalidCurrentPositionIdError()
    {
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(1, -1);

        var result = UpdatePositionValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == EmployeeValidationErrorEnum.InvalidCurrentPositionId);
    }

    [Fact]
    public void Validate_BothIdsInvalid_ReturnsTwoErrors()
    {
        var request = new UpdatePositionRequest { EmployeeId = 0, NewPositionId = 0 };

        var result = UpdatePositionValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
