using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.DAL;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EMI.EmployeeManagement.Tests.DAL;

public class PositionHistoryDALTests
{
    [Fact]
    public async Task UpdateEmployeePositionAsync_EmployeeNotFound_ThrowsDaoException()
    {
        await using var context = DbContextFactory.Create();
        var dal = new PositionHistoryDAL(context);
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(999, 1);

        var act = () => dal.UpdateEmployeePositionAsync(request, DateTime.UtcNow);

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*Employee not found*");
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_NewPositionNotFound_ThrowsDaoException()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        await TestDataBuilder.SeedActivePositionHistoryAsync(context, employee.EmployeeId, position.PositionId);
        var dal = new PositionHistoryDAL(context);
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(employee.EmployeeId, 999);

        var act = () => dal.UpdateEmployeePositionAsync(request, DateTime.UtcNow);

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*Position not found*");
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_NoActiveHistory_ThrowsDaoException()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var newPosition = await TestDataBuilder.SeedPositionAsync(context, name: "Manager");
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        var dal = new PositionHistoryDAL(context);
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(employee.EmployeeId, newPosition.PositionId);

        var act = () => dal.UpdateEmployeePositionAsync(request, DateTime.UtcNow);

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*Active position history not found*");
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_SamePosition_ThrowsDaoException()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        await TestDataBuilder.SeedActivePositionHistoryAsync(context, employee.EmployeeId, position.PositionId);
        var dal = new PositionHistoryDAL(context);
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(employee.EmployeeId, position.PositionId);

        var act = () => dal.UpdateEmployeePositionAsync(request, DateTime.UtcNow);

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*already has the position*");
    }

    [Fact]
    public async Task UpdateEmployeePositionAsync_ValidChange_ClosesOldHistoryAndCreatesNew()
    {
        await using var context = DbContextFactory.Create();
        var oldPosition = await TestDataBuilder.SeedPositionAsync(context, name: "Junior");
        var newPosition = await TestDataBuilder.SeedPositionAsync(context, name: "Senior");
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, oldPosition.PositionId);
        var oldHistory = await TestDataBuilder.SeedActivePositionHistoryAsync(context, employee.EmployeeId, oldPosition.PositionId);
        var dal = new PositionHistoryDAL(context);
        var startDate = DateTime.UtcNow;
        var request = TestDataBuilder.CreateValidUpdatePositionRequest(employee.EmployeeId, newPosition.PositionId);

        var newHistoryId = await dal.UpdateEmployeePositionAsync(request, startDate);

        newHistoryId.Should().BeGreaterThan(0);

        var closedHistory = await context.PositionHistories.FindAsync(oldHistory.PositionHistoryId);
        closedHistory!.PositionHistoryIsActive.Should().BeFalse();
        closedHistory.PositionHistoryEndDate.Should().NotBeNull();

        var activeHistory = await context.PositionHistories.FindAsync(newHistoryId);
        activeHistory!.PositionHistoryIsActive.Should().BeTrue();
        activeHistory.PositionHistoryPositionId.Should().Be(newPosition.PositionId);

        var updatedEmployee = await context.Employees.FindAsync(employee.EmployeeId);
        updatedEmployee!.EmployeeCurrentPositionId.Should().Be(newPosition.PositionId);
    }

    [Fact]
    public async Task CreateInitialPositionHistoryAsync_EmployeeNotFound_ThrowsDaoException()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var dal = new PositionHistoryDAL(context);

        var act = () => dal.CreateInitialPositionHistoryAsync(999, position.PositionId, DateTime.UtcNow);

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*Employee not found*");
    }

    [Fact]
    public async Task CreateInitialPositionHistoryAsync_PositionNotFound_ThrowsDaoException()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        var dal = new PositionHistoryDAL(context);

        var act = () => dal.CreateInitialPositionHistoryAsync(employee.EmployeeId, 999, DateTime.UtcNow);

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*Position not found*");
    }

    [Fact]
    public async Task CreateInitialPositionHistoryAsync_AlreadyHasActiveHistory_ThrowsDaoException()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        await TestDataBuilder.SeedActivePositionHistoryAsync(context, employee.EmployeeId, position.PositionId);
        var dal = new PositionHistoryDAL(context);

        var act = () => dal.CreateInitialPositionHistoryAsync(employee.EmployeeId, position.PositionId, DateTime.UtcNow);

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*already has an active position history*");
    }

    [Fact]
    public async Task CreateInitialPositionHistoryAsync_ValidRequest_AddsHistoryToContext()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        var dal = new PositionHistoryDAL(context);

        var historyId = await dal.CreateInitialPositionHistoryAsync(employee.EmployeeId, position.PositionId, DateTime.UtcNow);

        historyId.Should().BeGreaterThan(0);
        context.PositionHistories.Local.Should().ContainSingle(h => h.PositionHistoryEmployeeId == employee.EmployeeId);
    }

    [Fact]
    public async Task IsEmployeeManagerAsync_ManagerPosition_ReturnsTrue()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context, isManager: true, name: "Manager");
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        await TestDataBuilder.SeedActivePositionHistoryAsync(context, employee.EmployeeId, position.PositionId);
        var dal = new PositionHistoryDAL(context);

        var result = await dal.IsEmployeeManagerAsync(employee.EmployeeId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmployeeManagerAsync_NonManagerPosition_ReturnsFalse()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context, isManager: false);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        await TestDataBuilder.SeedActivePositionHistoryAsync(context, employee.EmployeeId, position.PositionId);
        var dal = new PositionHistoryDAL(context);

        var result = await dal.IsEmployeeManagerAsync(employee.EmployeeId);

        result.Should().BeFalse();
    }
}
