using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.BLL.Interfaces
{
    public interface IAuthBLL
    {
        Task<AuthUserDto?> AuthenticateAsync(string username, string password);
    }
}
