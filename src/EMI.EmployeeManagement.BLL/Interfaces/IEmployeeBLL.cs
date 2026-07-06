using EMI.EmployeeManagement.Common.Responses;
using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.BLL.Interfaces
{
    public interface IEmployeeBLL
    {
        Task<ResponseQuery<EmployeeResponse>> GetByIdAsync(int id);
        Task<ResponseList<EmployeeResponse>> GetAllAsync();
        Task<ResponseRegister> AddAsync(CreateEmployeeRequest imput);
        Task<ResponseBase> DeleteAsync(int id);
        Task<ResponseBase> UpdateAsync(UpdateEmployeeRequest input);
    }
}
