using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.DAL;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;

namespace EMI.EmployeeManagement.Tests.DAL;

public class AuthDALTests
{
    [Fact]
    public async Task AuthenticateAsync_UserNotFound_ReturnsNull()
    {
        await using var context = DbContextFactory.Create();
        var dal = new AuthDAL(context);

        var result = await dal.AuthenticateAsync("unknown", "pass");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsNull()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId, password: "abc123");
        var dal = new AuthDAL(context);

        var result = await dal.AuthenticateAsync("John Doe", "wrong");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentialsNoRoles_ReturnsUserWithEmptyRoles()
    {
        await using var context = DbContextFactory.Create();
        var position = await TestDataBuilder.SeedPositionAsync(context);
        await TestDataBuilder.SeedEmployeeAsync(context, position.PositionId, password: "secret");
        var dal = new AuthDAL(context);

        var result = await dal.AuthenticateAsync("John Doe", "secret");

        result.Should().NotBeNull();
        result!.Username.Should().Be("John Doe");
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentialsWithRoles_ReturnsUserWithRoles()
    {
        await using var context = DbContextFactory.Create();
        var (employee, role, _) = await TestDataBuilder.SeedFullEmployeeWithRoleAsync(context, password: "secret");
        var dal = new AuthDAL(context);

        var result = await dal.AuthenticateAsync(employee.EmployeeName, "secret");

        result.Should().NotBeNull();
        result!.UserId.Should().Be(employee.EmployeeId);
        result.Roles.Should().Contain(role.RoleName);
    }

    [Fact]
    public async Task AuthenticateAsync_DisposedContext_ThrowsDaoException()
    {
        var context = DbContextFactory.Create();
        context.Dispose();
        var dal = new AuthDAL(context);

        var act = () => dal.AuthenticateAsync("user", "pass");

        await act.Should().ThrowAsync<DaoException>()
            .WithMessage("*Error authenticating user*");
    }
}
