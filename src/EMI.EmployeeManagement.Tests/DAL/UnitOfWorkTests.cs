using EMI.EmployeeManagement.DAL;
using EMI.EmployeeManagement.DAL.UnitOfWorks;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;

namespace EMI.EmployeeManagement.Tests.DAL;

public class UnitOfWorkTests
{
    [Fact]
    public async Task CommitAsync_PersistsChanges()
    {
        await using var context = DbContextFactory.Create();
        var employeeDal = new EmployeeDAL(context);
        var positionHistoryDal = new PositionHistoryDAL(context);
        var uow = new UnitOfWork(context, employeeDal, positionHistoryDal);
        var position = await TestDataBuilder.SeedPositionAsync(context);

        await uow.BeginTransactionAsync();
        var request = TestDataBuilder.CreateValidEmployeeRequest(position.PositionId);
        var id = await uow.Employees.AddAsync(request);
        await uow.PositionHistories.CreateInitialPositionHistoryAsync(id, position.PositionId, DateTime.UtcNow);
        await uow.CommitAsync();

        (await context.Employees.FindAsync(id)).Should().NotBeNull();
    }

    [Fact]
    public async Task RollbackAsync_CompletesWithoutError()
    {
        await using var context = DbContextFactory.Create();
        var employeeDal = new EmployeeDAL(context);
        var positionHistoryDal = new PositionHistoryDAL(context);
        var uow = new UnitOfWork(context, employeeDal, positionHistoryDal);

        await uow.BeginTransactionAsync();
        var act = () => uow.RollbackAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_ExposesDalInstances()
    {
        var context = DbContextFactory.Create();
        var employeeDal = new EmployeeDAL(context);
        var positionHistoryDal = new PositionHistoryDAL(context);
        var uow = new UnitOfWork(context, employeeDal, positionHistoryDal);

        uow.Employees.Should().BeSameAs(employeeDal);
        uow.PositionHistories.Should().BeSameAs(positionHistoryDal);
    }
}
