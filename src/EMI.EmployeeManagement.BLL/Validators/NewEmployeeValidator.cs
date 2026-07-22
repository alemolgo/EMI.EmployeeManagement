using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Validation;
using EMI.EmployeeManagement.Common.Validations;
using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.BLL.Validators
{
    public static class NewEmployeeValidator
    {
        public static ValidationResult Validate(CreateEmployeeRequest request)
        {
            var errors = new List<ValidationErrorItem>();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidName,
                    Message = "Employee name is required."
                });
            }

            if (request.Name?.Length > 100)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidName,
                    Message = "Employee name cannot exceed 100 characters."
                });
            }

            if (request.CurrentPositionId <= 0)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidCurrentPositionId,
                    Message = "Invalid position id."
                });
            }

            if (request.Salary <= 0)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidSalary,
                    Message = "Salary must be greater than 0."
                });
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidPassword,
                    Message = "Invalid Employee Password"
                });
            }

            if (request.Password?.Length > 8)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidName,
                    Message = "Employee password cannot exceed 8 characters."
                });
            }

            if (request.RoleId <= 0)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidRole,
                    Message = "Invalid role id."
                });
            }

            if (errors.Any())
                return ValidationResult.Failure(errors.ToArray());

            return ValidationResult.Success();
        }
    }
}