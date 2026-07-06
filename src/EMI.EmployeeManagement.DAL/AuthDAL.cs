using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace EMI.EmployeeManagement.DAL
{
    public class AuthDAL : IAuthDAL
    {
        private readonly AppDbContext _context;

        public AuthDAL(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AuthUserDto?> AuthenticateAsync(string username, string password)
        {
            try
            {

                // 1. GET USER FROM DATABASE
                var user = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.EmployeeName == username);

                if (user == null)
                    return null;


                // 2. VALIDATE PASSWORD (TEMPORARY SIMPLE CHECK)              
                // In production: replace with BCrypt or Argon2
                var isValidPassword = user.EmployeePasswordHash == password;

                if (!isValidPassword)
                    return null;


                // 3. GET USER ROLES              
                var roles = await (
                    from er in _context.EmployeeRoles
                    join r in _context.Roles
                        on er.RoleId equals r.RoleId
                    where er.EmployeeId == user.EmployeeId
                    select r.RoleName
                ).ToListAsync();


                // 4. BUILD DTO
                return new AuthUserDto
                {
                    UserId = user.EmployeeId,
                    Username = user.EmployeeName,
                    Roles = roles
                };
            }
            catch (Exception ex)
            {
                throw new DaoException($"Error authenticating user: {ex.Message}");
            }
        }
    }
}