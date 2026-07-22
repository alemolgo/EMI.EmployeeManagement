using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.DAL;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EMI.EmployeeManagement.Tests.DAL;

public class EmployeeDALTests
{
    [Fact]
    public async Task AddAsync_PositionNotFound_ThrowsDaoException()
    {
        await using var context = DbContextFactory.Create();
        var dal = new EmployeeDAL(context);
        var request = TestDataBuilder.CreateValidEmployeeRequest(999);

        var act = () => dal.AddAsync(request);

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*Position not found*");
    }

    [Fact]
    public async Task AddAsync_ValidPosition_PersistsEmployeeAndReturnsId()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var role = await TestDataBuilder.SeedRoleAsync(context, "User");
        var dal = new EmployeeDAL(context);
        var request = TestDataBuilder.CreateValidEmployeeRequest(position.PositionId, role.RoleId);

        var id = await dal.AddAsync(request);

        id.Should().BeGreaterThan(0);
        var saved = await context.Employees.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.EmployeeName.Should().Be("John Doe");
        saved.EmployeeSalary.Should().Be(50000m);
        saved.EmployeeCurrentPositionId.Should().Be(position.PositionId);
        context.EmployeeRoles.Should().ContainSingle(er => er.EmployeeId == id);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEmployee_ReturnsMappedResponse()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        var dal = new EmployeeDAL(context);

        var result = await dal.GetByIdAsync(employee.EmployeeId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(employee.EmployeeId);
        result.Name.Should().Be(employee.EmployeeName);
        result.Salary.Should().Be(employee.EmployeeSalary);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEmployee_ReturnsNull()
    {
        await using var context = DbContextFactory.Create();
        var dal = new EmployeeDAL(context);

        var result = await dal.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_NoEmployees_ReturnsEmptyList()
    {
        await using var context = DbContextFactory.Create();
        var dal = new EmployeeDAL(context);

        var result = await dal.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithEmployees_ReturnsMappedList()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId, "Alice");
        await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId, "Bob");
        var dal = new EmployeeDAL(context);

        var result = await dal.GetAllAsync();

        result.Should().HaveCount(2);
        result.Select(e => e.Name).Should().Contain(new[] { "Alice", "Bob" });
    }

    [Fact]
    public async Task UpdateAsync_NonExistingEmployee_ReturnsFalse()
    {
        await using var context = DbContextFactory.Create();
        var dal = new EmployeeDAL(context);
        var request = TestDataBuilder.CreateValidUpdateEmployeeRequest(999);

        var result = await dal.UpdateAsync(request);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_OnlyName_UpdatesNameAndKeepsSalary()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId, salary: 50000m);
        var dal = new EmployeeDAL(context);
        var request = new UpdateEmployeeRequest { Id = employee.EmployeeId, Name = "Updated Name" };

        var result = await dal.UpdateAsync(request);

        result.Should().BeTrue();
        var updated = await context.Employees.FindAsync(employee.EmployeeId);
        updated!.EmployeeName.Should().Be("Updated Name");
        updated.EmployeeSalary.Should().Be(50000m);
    }

    [Fact]
    public async Task UpdateAsync_OnlySalary_UpdatesSalary()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        var dal = new EmployeeDAL(context);
        var request = new UpdateEmployeeRequest { Id = employee.EmployeeId, Salary = 75000m };

        var result = await dal.UpdateAsync(request);

        result.Should().BeTrue();
        var updated = await context.Employees.FindAsync(employee.EmployeeId);
        updated!.EmployeeSalary.Should().Be(75000m);
    }

    [Fact]
    public async Task DeleteByIdAsync_NonExistingEmployee_ReturnsFalse()
    {
        await using var context = DbContextFactory.Create();
        var dal = new EmployeeDAL(context);

        var result = await dal.DeleteByIdAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteByIdAsync_ExistingEmployee_RemovesFromDatabase()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        var dal = new EmployeeDAL(context);

        var result = await dal.DeleteByIdAsync(employee.EmployeeId);

        result.Should().BeTrue();
        (await context.Employees.FindAsync(employee.EmployeeId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteByIdAsync_EmployeeWithPositionHistory_RemovesEmployeeAndHistory()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        var employee = await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId);
        await TestDataBuilder.SeedActivePositionHistoryAsync(context, employee.EmployeeId, position.PositionId);
        var dal = new EmployeeDAL(context);

        var result = await dal.DeleteByIdAsync(employee.EmployeeId);

        result.Should().BeTrue();
        (await context.Employees.FindAsync(employee.EmployeeId)).Should().BeNull();
        context.PositionHistories.Where(h => h.PositionHistoryEmployeeId == employee.EmployeeId).Should().BeEmpty();
    }
}
