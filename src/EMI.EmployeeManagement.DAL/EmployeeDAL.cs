using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace EMI.EmployeeManagement.DAL
{
    public class EmployeeDAL : IEmployeeDAL
    {

        private readonly AppDbContext _context;

        public EmployeeDAL(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> AddAsync(CreateEmployeeRequest employeeRequest)
        {
            // 1. Validate position exists
            var positionExists = await _context.Positions
                .AsNoTracking()
                .AnyAsync(p => p.PositionId == employeeRequest.CurrentPositionId);

            if (!positionExists)
                throw new DaoException($"Position not found. PositionId: {employeeRequest.CurrentPositionId}");

            // 2. Validate role exists
            var roleExists = await _context.Roles
                .AsNoTracking()
                .AnyAsync(r => r.RoleId == employeeRequest.RoleId);

            if (!roleExists)
                throw new DaoException($"Role not found. RoleId: {employeeRequest.RoleId}");

            // 3. Create employee with role assignment
            var employee = new Employee
            {
                EmployeeName = employeeRequest.Name,
                EmployeeSalary = employeeRequest.Salary,
                EmployeeCurrentPositionId = employeeRequest.CurrentPositionId,
                EmployeePasswordHash = employeeRequest.Password
            };

            employee.EmployeeRoles.Add(new EmployeeRole
            {
                RoleId = employeeRequest.RoleId
            });

            // 4. Add to DbSet
            await _context.Employees.AddAsync(employee);

            await _context.SaveChangesAsync();
            return employee.EmployeeId;
        }

        public async Task<EmployeeResponse> GetByIdAsync(int id)
        {
            var e = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(emp => emp.EmployeeId == id);

            if (e == null) return null;

            return new EmployeeResponse
            {
                Id = e.EmployeeId,
                Name = e.EmployeeName,
                Salary = e.EmployeeSalary,
                CurrentPositionId = e.EmployeeCurrentPositionId
            };
        }

        public async Task<bool> DeleteByIdAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
                return false;

            var positionHistories = await _context.PositionHistories
                .Where(h => h.PositionHistoryEmployeeId == id)
                .ToListAsync();

            if (positionHistories.Count > 0)
                _context.PositionHistories.RemoveRange(positionHistories);

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<EmployeeResponse>> GetAllAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .Select(e => new EmployeeResponse
                {
                    Id = e.EmployeeId,
                    Name = e.EmployeeName,
                    Salary = e.EmployeeSalary,
                    CurrentPositionId = e.EmployeeCurrentPositionId
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(UpdateEmployeeRequest employeeRequest)
        {
            var e = await _context.Employees.Where(emp => emp.EmployeeId == employeeRequest.Id).FirstOrDefaultAsync();

            if (e == null) return false;

            if (!string.IsNullOrWhiteSpace(employeeRequest.Name))
                e.EmployeeName = employeeRequest.Name;

            if (employeeRequest.Salary.HasValue)
                e.EmployeeSalary = employeeRequest.Salary.Value;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
