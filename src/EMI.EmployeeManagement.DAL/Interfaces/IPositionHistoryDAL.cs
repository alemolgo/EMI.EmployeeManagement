using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.DAL.Interfaces
{
    public interface IPositionHistoryDAL
    {
        Task<int> UpdateEmployeePositionAsync(UpdatePositionRequest request, DateTime startDate);

        Task<int> CreateInitialPositionHistoryAsync(int employeeId, int positionId, DateTime startDate);

        Task<bool> IsEmployeeManagerAsync(int employeeId);
    }
}
