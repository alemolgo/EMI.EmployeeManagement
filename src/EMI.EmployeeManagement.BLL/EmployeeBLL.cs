using EMI.EmployeeManagement.BLL.Interfaces;
using EMI.EmployeeManagement.BLL.Validators;
using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.Common.Responses;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.DAL.UnitOfWorks;
using EMI.EmployeeManagement.Entities.Dto;
using Microsoft.Extensions.Logging;

namespace EMI.EmployeeManagement.BLL
{
    public class EmployeeBLL : IEmployeeBLL
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmployeeDAL _employeeDAL;
        private readonly IPositionHistoryDAL _positionDAL;
        private readonly ILogger<EmployeeBLL> _logger;

        public EmployeeBLL(IUnitOfWork uow, IEmployeeDAL employeeDAL, IPositionHistoryDAL positionDAL, ILogger<EmployeeBLL> logger)
        {
            _uow = uow;
            _employeeDAL = employeeDAL;
            _positionDAL = positionDAL;
            _logger = logger;
        }


        public async Task<ResponseRegister> AddAsync(CreateEmployeeRequest request)
        {
            var validation = NewEmployeeValidator.Validate(request);

            if (!validation.IsValid)
            {
                return new ResponseRegister
                {
                    Success = false,
                    ErrorCode = ErrorTypeEnum.BUSINESS_ERROR,
                    Message = "Validation failed.",
                    ValidationErrors = validation.Errors
                };
            }


            try
            {
                _logger.LogInformation("Starting employee creation...");

                await _uow.BeginTransactionAsync();

                // 1. Create employee
                var id = await _uow.Employees.AddAsync(request);

                // 2. Create initial position history
                await _uow.PositionHistories.CreateInitialPositionHistoryAsync(
                    id,
                    request.CurrentPositionId,
                    DateTime.UtcNow
                );

                await _uow.CommitAsync();

                _logger.LogInformation($"Employee created with Id {id}");

                return new ResponseRegister
                {
                    Success = true,
                    Id = id,
                    Message = "Employee created successfully."
                };

            }
            catch (DaoException de)
            {
                await _uow.RollbackAsync();

                _logger.LogError(de, "Database error creating employee");

                return new ResponseRegister
                {
                    Success = false,
                    Message = "Database error while creating employee.",
                    ErrorCode = ErrorTypeEnum.DB_ERROR
                };
            }
            catch (Exception e)
            {
                await _uow.RollbackAsync();

                _logger.LogError(e, "Unexpected error creating employee");

                return new ResponseRegister
                {
                    Success = false,
                    Message = "Unexpected error while creating employee.",
                    ErrorCode = ErrorTypeEnum.UNKNOWN_ERROR
                };
            }
        }

        public async Task<ResponseQuery<EmployeeResponse>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new ResponseQuery<EmployeeResponse>
                {
                    Success = false,
                    Message = "Employee id must be greater than zero.",
                    ErrorCode = ErrorTypeEnum.INVALID_ID
                };
            }

            try
            {
                _logger.LogInformation($"Getting employee with Id {id}...");

                var employee = await _employeeDAL.GetByIdAsync(id);


                if (employee == null)
                {
                    _logger.LogWarning($"Employee with Id {id} was not found.");

                    return new ResponseQuery<EmployeeResponse>
                    {
                        Success = false,
                        Message = $"Employee with Id {id} was not found.",
                        ErrorCode = ErrorTypeEnum.NOT_FOUND
                    };
                }


                bool IsEmployeeManager = await _positionDAL.IsEmployeeManagerAsync(id);
                employee.bonus = CalculateYearlyBonus(employee.Salary, IsEmployeeManager);

                _logger.LogInformation($"Employee with Id {id} retrieved successfully.");

                return new ResponseQuery<EmployeeResponse>
                {
                    Success = true,
                    Data = employee
                };
            }
            catch (DaoException de)
            {
                _logger.LogError(de, $"Database error getting employee with Id {id}");

                return new ResponseQuery<EmployeeResponse>
                {
                    Success = false,
                    Message = "Database error while retrieving employee.",
                    ErrorCode = ErrorTypeEnum.DB_ERROR
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unexpected error getting employee with Id {id}");

                return new ResponseQuery<EmployeeResponse>
                {
                    Success = false,
                    Message = "Unexpected error while retrieving employee.",
                    ErrorCode = ErrorTypeEnum.UNKNOWN_ERROR
                };
            }
        }

        public async Task<ResponseBase> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new ResponseBase
                {
                    Success = false,
                    Message = "Employee id must be greater than zero.",
                    ErrorCode = ErrorTypeEnum.INVALID_ID
                };
            }

            try
            {
                _logger.LogInformation($"Deleting employee with Id {id}...");

                var deleted = await _employeeDAL.DeleteByIdAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning($"Employee with Id {id} not found for deletion.");

                    return new ResponseBase
                    {
                        Success = false,
                        Message = $"Employee with Id {id} was not found.",
                        ErrorCode = ErrorTypeEnum.NOT_FOUND
                    };
                }

                _logger.LogInformation($"Employee with Id {id} deleted successfully.");

                return new ResponseBase
                {
                    Success = true,
                    Message = "Employee deleted successfully."
                };
            }
            catch (DaoException de)
            {
                _logger.LogError(de, $"Error deleting employee with Id {id}");

                return new ResponseBase
                {
                    Success = false,
                    Message = $"Database Error Deleting Employee: {de.Message}",
                    ErrorCode = ErrorTypeEnum.DB_ERROR
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting employee with Id {id}");

                return new ResponseBase
                {
                    Success = false,
                    Message = "Unexpected error while deleting employee.",
                    ErrorCode = ErrorTypeEnum.UNKNOWN_ERROR
                };
            }
        }

        public async Task<ResponseList<EmployeeResponse>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all employees...");

                var employees = await _employeeDAL.GetAllAsync();

                if (employees == null || !employees.Any())
                {
                    _logger.LogInformation("No employees found.");

                    return new ResponseList<EmployeeResponse>
                    {
                        Success = true,
                        Data = Enumerable.Empty<EmployeeResponse>(),
                        Message = "No employees found."
                    };
                }

                _logger.LogInformation($"Retrieved {employees.Count()} employees successfully.");

                return new ResponseList<EmployeeResponse>
                {
                    Success = true,
                    Data = employees
                };
            }
            catch (DaoException de)
            {
                _logger.LogError(de, "Database error retrieving employees");

                return new ResponseList<EmployeeResponse>
                {
                    Success = false,
                    Message = "Database error while retrieving employees.",
                    ErrorCode = ErrorTypeEnum.DB_ERROR
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error retrieving employees");

                return new ResponseList<EmployeeResponse>
                {
                    Success = false,
                    Message = "Unexpected error while retrieving employees.",
                    ErrorCode = ErrorTypeEnum.UNKNOWN_ERROR
                };
            }
        }

        public async Task<ResponseBase> UpdateAsync(UpdateEmployeeRequest request)
        {

            var validation = UpdateEmployeeValidator.Validate(request);

            if (!validation.IsValid)
            {
                return new ResponseBase
                {
                    Success = false,
                    Message = "Validation failed.",
                    ErrorCode = ErrorTypeEnum.INVALID_REQUEST,
                    ValidationErrors = validation.Errors
                };
            }


            try
            {
                _logger.LogInformation($"Updating employee with Id {request.Id}...");

                var updated = await _employeeDAL.UpdateAsync(request);

                if (!updated)
                {
                    _logger.LogWarning($"Employee with Id {request.Id} was not found.");

                    return new ResponseBase
                    {
                        Success = false,
                        Message = $"Employee with Id {request.Id} was not found.",
                        ErrorCode = ErrorTypeEnum.NOT_FOUND
                    };
                }

                _logger.LogInformation($"Employee with Id {request.Id} updated successfully.");

                return new ResponseBase
                {
                    Success = true,
                    Message = "Employee updated successfully."
                };
            }
            catch (DaoException de)
            {
                _logger.LogError(de, $"Database error updating employee with Id {request.Id}");

                return new ResponseBase
                {
                    Success = false,
                    Message = "Database error while updating employee.",
                    ErrorCode = ErrorTypeEnum.DB_ERROR
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unexpected error updating employee with Id {request.Id}");

                return new ResponseBase
                {
                    Success = false,
                    Message = "Unexpected error while updating employee.",
                    ErrorCode = ErrorTypeEnum.UNKNOWN_ERROR
                };
            }
        }

        private decimal CalculateYearlyBonus(decimal salary, bool isManager)
        {
            var bonus = isManager ? salary * 0.20m : salary * 0.10m;
            return Math.Round(bonus, 2);
        }
    }

}