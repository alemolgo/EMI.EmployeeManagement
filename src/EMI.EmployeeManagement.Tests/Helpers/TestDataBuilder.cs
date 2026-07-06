using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Entities.Models;

namespace EMI.EmployeeManagement.Tests.Helpers;

public static class TestDataBuilder
{
    public static CreateEmployeeRequest CreateValidEmployeeRequest(int positionId = 1) =>
        new()
        {
            Name = "John Doe",
            CurrentPositionId = positionId,
            Salary = 50000m,
            Password = "pass123"
        };

    public static UpdateEmployeeRequest CreateValidUpdateEmployeeRequest(int id) =>
        new()
        {
            Id = id,
            Name = "Jane Doe",
            Salary = 60000m
        };

    public static UpdatePositionRequest CreateValidUpdatePositionRequest(int employeeId, int newPositionId = 2) =>
        new()
        {
            EmployeeId = employeeId,
            NewPositionId = newPositionId
        };

    public static EmployeeResponse CreateEmployeeResponse(int id, decimal salary) =>
        new()
        {
            Id = id,
            Name = "John Doe",
            Salary = salary,
            CurrentPositionId = 1
        };

    public static AuthUserDto CreateAuthUserDto() =>
        new()
        {
            UserId = 1,
            Username = "john",
            Roles = new List<string> { "Admin" }
        };

    public static async Task<Position> SeedPositionAsync(AppDbContext context, bool isManager = false, string name = "Developer")
    {
        var position = new Position
        {
            PositionName = name,
            PositionIsManager = isManager
        };

        context.Positions.Add(position);
        await context.SaveChangesAsync();
        return position;
    }

    public static async Task<Role> SeedRoleAsync(AppDbContext context, string name = "Admin")
    {
        var role = new Role { RoleName = name };
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        return role;
    }

    public static async Task<Employee> SeedEmployeeAsync(
        AppDbContext context,
        int positionId,
        string name = "John Doe",
        string password = "pass123",
        decimal salary = 50000m)
    {
        var employee = new Employee
        {
            EmployeeName = name,
            EmployeeCurrentPositionId = positionId,
            EmployeeSalary = salary,
            EmployeePasswordHash = password
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        return employee;
    }

    public static async Task<EmployeeRole> SeedEmployeeRoleAsync(AppDbContext context, int employeeId, int roleId)
    {
        var employeeRole = new EmployeeRole
        {
            EmployeeId = employeeId,
            RoleId = roleId
        };

        context.EmployeeRoles.Add(employeeRole);
        await context.SaveChangesAsync();
        return employeeRole;
    }

    public static async Task<PositionHistory> SeedActivePositionHistoryAsync(
        AppDbContext context,
        int employeeId,
        int positionId,
        DateTime? startDate = null)
    {
        var history = new PositionHistory
        {
            PositionHistoryEmployeeId = employeeId,
            PositionHistoryPositionId = positionId,
            PositionHistoryIsActive = true,
            PositionHistoryStartDate = startDate ?? DateTime.UtcNow
        };

        context.PositionHistories.Add(history);
        await context.SaveChangesAsync();
        return history;
    }

    public static async Task<(Employee Employee, Role Role, Position Position)> SeedFullEmployeeWithRoleAsync(
        AppDbContext context,
        bool isManager = false,
        string password = "pass123")
    {
        var position = await SeedPositionAsync(context, isManager);
        var role = await SeedRoleAsync(context);
        var employee = await SeedEmployeeAsync(context, position.PositionId, password: password);
        await SeedEmployeeRoleAsync(context, employee.EmployeeId, role.RoleId);
        await SeedActivePositionHistoryAsync(context, employee.EmployeeId, position.PositionId);
        return (employee, role, position);
    }
}
