using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.Entities.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace EMI.EmployeeManagement.DAL.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public IEmployeeDAL Employees { get; }
        public IPositionHistoryDAL PositionHistories { get; }

        public UnitOfWork(AppDbContext context, IEmployeeDAL employees, IPositionHistoryDAL positionHistories)
        {
            _context = context;
            Employees = employees;
            PositionHistories = positionHistories;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
            await _transaction!.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await _transaction!.RollbackAsync();
        }
    }
}
