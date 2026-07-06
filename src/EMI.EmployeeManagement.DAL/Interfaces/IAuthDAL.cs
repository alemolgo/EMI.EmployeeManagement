using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.DAL.Interfaces
{
    public interface IAuthDAL
    {
        Task<AuthUserDto?> AuthenticateAsync(string username, string password);
    }
}
