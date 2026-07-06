using EMI.EmployeeManagement.BLL;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EMI.EmployeeManagement.Tests.BLL;

public class PositionHistoryBLLTests
{
    private readonly Mock<IPositionHistoryDAL> _positionHistoryDal = new();
    private readonly PositionHistoryBLL _sut;

    public PositionHistoryBLLTests()
    {
        _sut = new PositionHistoryBLL(_positionHistoryDal.Object, BllMockSetup.CreateNullLogger<PositionHistoryBLL>());
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_ValidationFails_ReturnsInvalidRequestWithoutCallingDal()
    {
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(0, 2);

        var result = await _sut.UpdateEmployeePositionAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.INVALID_REQUEST);
        _positionHistoryDal.Verify(x => x.UpdateEmployeePositionAsync(It.IsAny<UpdatePositionRequest>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_Success_ReturnsRegisterWithId()
    {
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(1, 2);
        _positionHistoryDal.Setup(x => x.UpdateEmployeePositionAsync(request, It.IsAny<DateTime>())).ReturnsAsync(99);

        var result = await _sut.UpdateEmployeePositionAsync(request);

        result.Success.Should().BeTrue();
        result.Id.Should().Be(99);
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_DbUpdateException_ReturnsDbError()
    {
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(1, 2);
        _positionHistoryDal.Setup(x => x.UpdateEmployeePositionAsync(request, It.IsAny<DateTime>()))
            .ThrowsAsync(new DbUpdateException("db"));

        var result = await _sut.UpdateEmployeePositionAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.DB_ERROR);
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_UnexpectedException_ReturnsUnknownError()
    {
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(1, 2);
        _positionHistoryDal.Setup(x => x.UpdateEmployeePositionAsync(request, It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException());

        var result = await _sut.UpdateEmployeePositionAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.UNKNOWN_ERROR);
    }
}
