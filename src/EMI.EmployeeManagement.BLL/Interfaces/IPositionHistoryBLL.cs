using EMI.EmployeeManagement.Common.Responses;
using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.BLL.Interfaces
{
    public interface IPositionHistoryBLL
    {
        Task<ResponseRegister> UpdateEmployeePositionAsync(UpdatePositionRequest request);
    }
}
