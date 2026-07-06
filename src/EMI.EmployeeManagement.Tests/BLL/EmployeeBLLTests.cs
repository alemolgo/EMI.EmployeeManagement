using EMI.EmployeeManagement.BLL;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.DAL.UnitOfWorks;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace EMI.EmployeeManagement.Tests.BLL;

public class EmployeeBLLTests
{
    private readonly EmployeeBLL _sut;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IEmployeeDAL> _employeeDal;
    private readonly Mock<IPositionHistoryDAL> _positionHistoryDal;

    public EmployeeBLLTests()
    {
        (_uow, _employeeDal, _positionHistoryDal, var logger) = BllMockSetup.CreateEmployeeBLLMocks();
        _sut = new EmployeeBLL(_uow.Object, _employeeDal.Object, _positionHistoryDal.Object, logger.Object);
    }

    [Fact]
    public async Task AddAsync_ValidationFails_ReturnsBusinessErrorWithoutCallingUow()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        request.Name = "";

        var result = await _sut.AddAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.BUSINESS_ERROR);
        _uow.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task AddAsync_Success_CommitsTransactionAndReturnsId()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        _uow.Setup(x => x.Employees.AddAsync(request)).ReturnsAsync(42);
        _uow.Setup(x => x.PositionHistories.CreateInitialPositionHistoryAsync(42, request.CurrentPositionId, It.IsAny<DateTime>()))
            .ReturnsAsync(1);

        var result = await _sut.AddAsync(request);

        result.Success.Should().BeTrue();
        result.Id.Should().Be(42);
        _uow.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _uow.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_DaoException_RollsBackAndReturnsDbError()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        _uow.Setup(x => x.Employees.AddAsync(request)).ThrowsAsync(new DaoException("db error"));

        var result = await _sut.AddAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.DB_ERROR);
        _uow.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_UnexpectedException_RollsBackAndReturnsUnknownError()
    {
        var request = TestDataBuilder.CreateValidEmployeeRequest();
        _uow.Setup(x => x.Employees.AddAsync(request)).ThrowsAsync(new InvalidOperationException());

        var result = await _sut.AddAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.UNKNOWN_ERROR);
        _uow.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsInvalidIdWithoutCallingDal()
    {
        var result = await _sut.GetByIdAsync(0);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.INVALID_ID);
        _employeeDal.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNotFound()
    {
        _employeeDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((EmployeeResponse?)null);

        var result = await _sut.GetByIdAsync(1);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.NOT_FOUND);
    }

    [Fact]
    public async Task GetByIdAsync_NonManager_Calculates10PercentBonus()
    {
        var employee = TestDataBuilder.CreateEmployeeResponse(1, 10000m);
        _employeeDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        _positionHistoryDal.Setup(x => x.IsEmployeeManagerAsync(1)).ReturnsAsync(false);

        var result = await _sut.GetByIdAsync(1);

        result.Success.Should().BeTrue();
        result.Data!.bonus.Should().Be(1000m);
    }

    [Fact]
    public async Task GetByIdAsync_Manager_Calculates20PercentBonus()
    {
        var employee = TestDataBuilder.CreateEmployeeResponse(1, 10000m);
        _employeeDal.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        _positionHistoryDal.Setup(x => x.IsEmployeeManagerAsync(1)).ReturnsAsync(true);

        var result = await _sut.GetByIdAsync(1);

        result.Success.Should().BeTrue();
        result.Data!.bonus.Should().Be(2000m);
    }

    [Fact]
    public async Task GetByIdAsync_DaoException_ReturnsDbError()
    {
        _employeeDal.Setup(x => x.GetByIdAsync(1)).ThrowsAsync(new DaoException("db"));

        var result = await _sut.GetByIdAsync(1);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.DB_ERROR);
    }

    [Fact]
    public async Task GetByIdAsync_UnexpectedException_ReturnsUnknownError()
    {
        _employeeDal.Setup(x => x.GetByIdAsync(1)).ThrowsAsync(new InvalidOperationException());

        var result = await _sut.GetByIdAsync(1);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.UNKNOWN_ERROR);
    }

    [Fact]
    public async Task DeleteAsync_InvalidId_ReturnsInvalidId()
    {
        var result = await _sut.DeleteAsync(0);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.INVALID_ID);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsNotFound()
    {
        _employeeDal.Setup(x => x.DeleteByIdAsync(1)).ReturnsAsync(false);

        var result = await _sut.DeleteAsync(1);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.NOT_FOUND);
    }

    [Fact]
    public async Task DeleteAsync_Success_ReturnsSuccess()
    {
        _employeeDal.Setup(x => x.DeleteByIdAsync(1)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_DaoException_ReturnsDbError()
    {
        _employeeDal.Setup(x => x.DeleteByIdAsync(1)).ThrowsAsync(new DaoException("db"));

        var result = await _sut.DeleteAsync(1);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.DB_ERROR);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsSuccessWithEmptyData()
    {
        _employeeDal.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<EmployeeResponse>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
        result.Message.Should().Contain("No employees found");
    }

    [Fact]
    public async Task GetAllAsync_WithEmployees_ReturnsSuccessWithData()
    {
        var employees = new List<EmployeeResponse>
        {
            TestDataBuilder.CreateEmployeeResponse(1, 50000m),
            TestDataBuilder.CreateEmployeeResponse(2, 60000m)
        };
        _employeeDal.Setup(x => x.GetAllAsync()).ReturnsAsync(employees);

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_DaoException_ReturnsDbError()
    {
        _employeeDal.Setup(x => x.GetAllAsync()).ThrowsAsync(new DaoException("db"));

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.DB_ERROR);
    }

    [Fact]
    public async Task UpdateAsync_ValidationFails_ReturnsInvalidRequest()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(0);

        var result = await _sut.UpdateAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.INVALID_REQUEST);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNotFound()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(1);
        _employeeDal.Setup(x => x.UpdateAsync(request)).ReturnsAsync(false);

        var result = await _sut.UpdateAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateAsync_Success_ReturnsSuccess()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(1);
        _employeeDal.Setup(x => x.UpdateAsync(request)).ReturnsAsync(true);

        var result = await _sut.UpdateAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_DaoException_ReturnsDbError()
    {
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(1);
        _employeeDal.Setup(x => x.UpdateAsync(request)).ThrowsAsync(new DaoException("db"));

        var result = await _sut.UpdateAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorTypeEnum.DB_ERROR);
    }
}
