using EMI.EmployeeManagement.BLL;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.DAL.UnitOfWorks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EMI.EmployeeManagement.Tests.Helpers;

public static class BllMockSetup
{
    public static ILogger<T> CreateNullLogger<T>() => NullLogger<T>.Instance;

    public static (
        Mock<IUnitOfWork> Uow,
        Mock<IEmployeeDAL> EmployeeDal,
        Mock<IPositionHistoryDAL> PositionHistoryDal,
        Mock<ILogger<EmployeeBLL>> Logger) CreateEmployeeBLLMocks()
    {
        var uow = new Mock<IUnitOfWork>();
        var employeeDal = new Mock<IEmployeeDAL>();
        var positionHistoryDal = new Mock<IPositionHistoryDAL>();
        var logger = new Mock<ILogger<EmployeeBLL>>();

        uow.SetupGet(x => x.Employees).Returns(employeeDal.Object);
        uow.SetupGet(x => x.PositionHistories).Returns(positionHistoryDal.Object);

        return (uow, employeeDal, positionHistoryDal, logger);
    }
}
