using EMI.EmployeeManagement.DAL.Interfaces;

namespace EMI.EmployeeManagement.DAL.UnitOfWorks
{
    public interface IUnitOfWork
    {
        IEmployeeDAL Employees { get; }
        IPositionHistoryDAL PositionHistories { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
