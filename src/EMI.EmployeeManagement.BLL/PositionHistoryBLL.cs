using EMI.EmployeeManagement.BLL.Interfaces;
using EMI.EmployeeManagement.BLL.Validators;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Responses;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.Entities.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EMI.EmployeeManagement.BLL
{
    public class PositionHistoryBLL : IPositionHistoryBLL
    {
        private readonly IPositionHistoryDAL _positionHistoryDAL;
        private readonly ILogger<PositionHistoryBLL> _logger;

        public PositionHistoryBLL(
            IPositionHistoryDAL positionHistoryDAL,
            ILogger<PositionHistoryBLL> logger)
        {
            _positionHistoryDAL = positionHistoryDAL;
            _logger = logger;
        }

        public async Task<ResponseRegister> UpdateEmployeePositionAsync(UpdatePositionRequest request)
        {
            var validation = UpdatePositionValidator.Validate(request);

            if (!validation.IsValid)
            {
                return new ResponseRegister
                {
                    Success = false,
                    Message = "Validation failed.",
                    ErrorCode = ErrorTypeEnum.INVALID_REQUEST,
                    ValidationErrors = validation.Errors
                };
            }


            try
            {
                _logger.LogInformation($"Changing position for employee {request.EmployeeId}");

                var createdPositionId = await _positionHistoryDAL.UpdateEmployeePositionAsync(request, DateTime.UtcNow);

                _logger.LogInformation($"Employee position changed successfully to {request.NewPositionId}");

                return new ResponseRegister
                {
                    Success = true,
                    Id = createdPositionId,
                    Message = $"Employee position changed successfully to {request.NewPositionId}"
                };
            }

            catch (DbUpdateException du)
            {
                _logger.LogError(du, "Database Update error changing employee position");

                return new ResponseRegister
                {
                    Success = false,
                    Message = "Database error while changing employee position.",
                    ErrorCode = ErrorTypeEnum.DB_ERROR
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error changing employee position.");

                return new ResponseRegister
                {
                    Success = false,
                    Message = "Unexpected error while changing employee position.",
                    ErrorCode = ErrorTypeEnum.UNKNOWN_ERROR
                };
            }
        }
    }
}