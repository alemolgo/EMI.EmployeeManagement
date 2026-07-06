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

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeBLL> _employeeBll = new();
    private readonly EmployeesController _sut;

    public EmployeesControllerTests()
    {
        _sut = new EmployeesController(_employeeBll.Object);
    }

    [Fact]
    public async Task AddAsync_Success_ReturnsCreatedAtRoute()
    {
        _employeeBll.Setup(x => x.AddAsync(It.IsAny<CreateEmployeeRequest>()))
            .ReturnsAsync(new ResponseRegister { Success = true, Id = 1 });

        var result = await _sut.AddAsync(TestDataBuilder.CreateValidEmployeeRequest());

        result.Should().BeOfType<CreatedAtRouteResult>();
        ((CreatedAtRouteResult)result).StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task AddAsync_BusinessError_ReturnsUnprocessableEntity()
    {
        _employeeBll.Setup(x => x.AddAsync(It.IsAny<CreateEmployeeRequest>()))
            .ReturnsAsync(new ResponseRegister { Success = false, ErrorCode = ErrorTypeEnum.BUSINESS_ERROR });

        var result = await _sut.AddAsync(TestDataBuilder.CreateValidEmployeeRequest());

        result.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    [Fact]
    public async Task AddAsync_DbError_Returns500()
    {
        _employeeBll.Setup(x => x.AddAsync(It.IsAny<CreateEmployeeRequest>()))
            .ReturnsAsync(new ResponseRegister { Success = false, ErrorCode = ErrorTypeEnum.DB_ERROR });

        var result = await _sut.AddAsync(TestDataBuilder.CreateValidEmployeeRequest());

        var statusResult = result as ObjectResult;
        statusResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetByIdAsync_Success_ReturnsOk()
    {
        _employeeBll.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new ResponseQuery<EmployeeResponse> { Success = true, Data = TestDataBuilder.CreateEmployeeResponse(1, 50000m) });

        var result = await _sut.GetByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
    {
        _employeeBll.Setup(x => x.GetByIdAsync(0))
            .ReturnsAsync(new ResponseQuery<EmployeeResponse> { Success = false, ErrorCode = ErrorTypeEnum.INVALID_ID });

        var result = await _sut.GetByIdAsync(0);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNotFound()
    {
        _employeeBll.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new ResponseQuery<EmployeeResponse> { Success = false, ErrorCode = ErrorTypeEnum.NOT_FOUND });

        var result = await _sut.GetByIdAsync(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAllAsync_Success_ReturnsOk()
    {
        _employeeBll.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new ResponseList<EmployeeResponse> { Success = true, Data = new List<EmployeeResponse>() });

        var result = await _sut.GetAllAsync();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllAsync_Failure_Returns500()
    {
        _employeeBll.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new ResponseList<EmployeeResponse> { Success = false, ErrorCode = ErrorTypeEnum.DB_ERROR });

        var result = await _sut.GetAllAsync();

        var statusResult = result as ObjectResult;
        statusResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteAsync_Success_ReturnsNoContent()
    {
        _employeeBll.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(new ResponseBase { Success = true });

        var result = await _sut.DeleteAsync(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsNotFound()
    {
        _employeeBll.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(new ResponseBase { Success = false, ErrorCode = ErrorTypeEnum.NOT_FOUND });

        var result = await _sut.DeleteAsync(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateAsync_Success_ReturnsNoContent()
    {
        _employeeBll.Setup(x => x.UpdateAsync(It.IsAny<UpdateEmployeeRequest>()))
            .ReturnsAsync(new ResponseBase { Success = true });

        var result = await _sut.UpdateAsync(5, new UpdateEmployeeRequest { Name = "Updated" });

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ReturnsBadRequest()
    {
        _employeeBll.Setup(x => x.UpdateAsync(It.IsAny<UpdateEmployeeRequest>()))
            .ReturnsAsync(new ResponseBase { Success = false, ErrorCode = ErrorTypeEnum.INVALID_REQUEST });

        var result = await _sut.UpdateAsync(5, new UpdateEmployeeRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateAsync_SetsIdFromRoute()
    {
        UpdateEmployeeRequest? captured = null;
        _employeeBll.Setup(x => x.UpdateAsync(It.IsAny<UpdateEmployeeRequest>()))
            .Callback<UpdateEmployeeRequest>(r => captured = r)
            .ReturnsAsync(new ResponseBase { Success = true });

        await _sut.UpdateAsync(7, new UpdateEmployeeRequest { Name = "Test" });

        captured!.Id.Should().Be(7);
    }
}
