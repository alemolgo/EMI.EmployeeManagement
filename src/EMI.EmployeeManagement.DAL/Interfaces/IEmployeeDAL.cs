using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.DAL.Interfaces
{
    public interface IEmployeeDAL
    {
        Task<EmployeeResponse> GetByIdAsync(int id);
        Task<List<EmployeeResponse>> GetAllAsync();
        Task<int> AddAsync(CreateEmployeeRequest employeeRequest);
        Task<bool> UpdateAsync(UpdateEmployeeRequest employeeRequest);
        Task<bool> DeleteByIdAsync(int id);
    }
}
