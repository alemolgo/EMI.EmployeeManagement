using EMI.EmployeeManagement.API.Controllers;
using EMI.EmployeeManagement.BLL.Interfaces;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Responses;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EMI.EmployeeManagement.Tests.API.Controllers;

public class PositionHistoryControllerTests
{
    private readonly Mock<IPositionHistoryBLL> _positionHistoryBll = new();
    private readonly PositionHistoryController _sut;

    public PositionHistoryControllerTests()
    {
        _sut = new PositionHistoryController(_positionHistoryBll.Object);
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_Success_ReturnsNoContent()
    {
        _positionHistoryBll.Setup(x => x.UpdateEmployeePositionAsync(It.IsAny<UpdatePositionRequest>()))
            .ReturnsAsync(new ResponseRegister { Success = true, Id = 1 });

        var result = await _sut.UpdateEmployeePositionAsync(1, new UpdatePositionRequest { NewPositionId = 2 });

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_InvalidRequest_ReturnsBadRequest()
    {
        _positionHistoryBll.Setup(x => x.UpdateEmployeePositionAsync(It.IsAny<UpdatePositionRequest>()))
            .ReturnsAsync(new ResponseRegister { Success = false, ErrorCode = ErrorTypeEnum.INVALID_REQUEST });

        var result = await _sut.UpdateEmployeePositionAsync(1, new UpdatePositionRequest { NewPositionId = 0 });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_DbError_Returns500()
    {
        _positionHistoryBll.Setup(x => x.UpdateEmployeePositionAsync(It.IsAny<UpdatePositionRequest>()))
            .ReturnsAsync(new ResponseRegister { Success = false, ErrorCode = ErrorTypeEnum.DB_ERROR });

        var result = await _sut.UpdateEmployeePositionAsync(1, new UpdatePositionRequest { NewPositionId = 2 });

        var statusResult = result as ObjectResult;
        statusResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_SetsEmployeeIdFromRoute()
    {
        UpdatePositionRequest? captured = null;
        _positionHistoryBll.Setup(x => x.UpdateEmployeePositionAsync(It.IsAny<UpdatePositionRequest>()))
            .Callback<UpdatePositionRequest>(r => captured = r)
            .ReturnsAsync(new ResponseRegister { Success = true });

        await _sut.UpdateEmployeePositionAsync(5, new UpdatePositionRequest { NewPositionId = 2 });

        captured!.EmployeeId.Should().Be(5);
    }
}
