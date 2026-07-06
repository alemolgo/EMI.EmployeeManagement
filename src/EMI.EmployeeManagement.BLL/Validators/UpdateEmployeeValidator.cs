using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Validation;
using EMI.EmployeeManagement.Common.Validations;
using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.BLL.Validators
{
    public static class UpdateEmployeeValidator
    {
        public static ValidationResult Validate(UpdateEmployeeRequest request)
        {
            var errors = new List<ValidationErrorItem>();

         
            if (request.Id <= 0)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidId,
                    Message = "Employee Id must be greater than zero."
                });
            }

           
            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 100)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidName,
                    Message = "Employee name cannot exceed 100 characters."
                });
            }

           
            if (request.Salary.HasValue && request.Salary <= 0)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidSalary,
                    Message = "Salary must be greater than 0."
                });
            }

            return errors.Any()
                ? ValidationResult.Failure(errors.ToArray())
                : ValidationResult.Success();
        }
    }
}