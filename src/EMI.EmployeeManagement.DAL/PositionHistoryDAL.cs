using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Entities.Models;
using Microsoft.EntityFrameworkCore;


namespace EMI.EmployeeManagement.DAL
{
    public class PositionHistoryDAL : IPositionHistoryDAL
    {

        private readonly AppDbContext _context;

        public PositionHistoryDAL(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        /// <summary>
        /// Changes the employee's current position.
        /// Closes the active position history,
        /// creates a new history record,
        /// and updates the employee's current position,
        /// all within a single transaction.
        /// </summary>
        public async Task<int> UpdateEmployeePositionAsync(UpdatePositionRequest request, DateTime startDate)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Get employee
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(emp => emp.EmployeeId == request.EmployeeId);

                if (employee == null)
                    throw new DaoException($"Employee not found. EmployeeId: {request.EmployeeId}");

                // 2. Validate new position
                var positionExists = await _context.Positions
                    .AnyAsync(p => p.PositionId == request.NewPositionId);

                if (!positionExists)
                    throw new DaoException($"Position not found. PositionId: {request.NewPositionId}");

                // 3. Get active position history
                var currentPosition = await _context.PositionHistories
                    .FirstOrDefaultAsync(hist =>
                        hist.PositionHistoryEmployeeId == request.EmployeeId &&
                        hist.PositionHistoryIsActive);

                if (currentPosition == null)
                    throw new DaoException($"Active position history not found for employee {request.EmployeeId}.");

                // 4. Validate business rule
                if (request.NewPositionId == currentPosition.PositionHistoryPositionId)
                    throw new DaoException($"Employee already has the position {request.NewPositionId}.");

                // 5. Close current history
                currentPosition.PositionHistoryEndDate = startDate.AddSeconds(-1);
                currentPosition.PositionHistoryIsActive = false;

                // 6. Update current employee position
                employee.EmployeeCurrentPositionId = request.NewPositionId;

                // 7. Persist changes before creating the new active history
                await _context.SaveChangesAsync();

                // 8. Create new active history
                var newPosition = new PositionHistory
                {
                    PositionHistoryEmployeeId = request.EmployeeId,
                    PositionHistoryPositionId = request.NewPositionId,
                    PositionHistoryStartDate = startDate,
                    PositionHistoryIsActive = true
                };

                await _context.PositionHistories.AddAsync(newPosition);

                // 9. Persist new history
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return newPosition.PositionHistoryId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> CreateInitialPositionHistoryAsync(int employeeId, int positionId, DateTime startDate)
        {
            // 1. Validate employee exists
            var employeeExists = await _context.Employees
                .AnyAsync(e => e.EmployeeId == employeeId);

            if (!employeeExists)
                throw new DaoException($"Employee not found. EmployeeId: {employeeId}");

            // 2. Validate position exists
            var positionExists = await _context.Positions
                .AnyAsync(p => p.PositionId == positionId);

            if (!positionExists)
                throw new DaoException($"Position not found. PositionId: {positionId}");

            // 3. Ensure no active history already
            var hasActiveHistory = await _context.PositionHistories
                .AnyAsync(h =>
                    h.PositionHistoryEmployeeId == employeeId &&
                    h.PositionHistoryIsActive);

            if (hasActiveHistory)
                throw new DaoException($"Employee {employeeId} already has an active position history.");

            // 4. Create record (NO transaction, NO commit)
            var history = new PositionHistory
            {
                PositionHistoryEmployeeId = employeeId,
                PositionHistoryPositionId = positionId,
                PositionHistoryStartDate = startDate,
                PositionHistoryIsActive = true
            };

            await _context.PositionHistories.AddAsync(history);

            return history.PositionHistoryId;
        }

        public async Task<bool> IsEmployeeManagerAsync(int employeeId)
        {
            var isManager = await (
                from ph in _context.PositionHistories
                join p in _context.Positions
                on ph.PositionHistoryPositionId equals p.PositionId
                where ph.PositionHistoryEmployeeId == employeeId && ph.PositionHistoryIsActive
                select p.PositionIsManager)
                .FirstOrDefaultAsync();

            return isManager;
        }
    }
}