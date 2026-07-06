using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Validation;
using EMI.EmployeeManagement.Common.Validations;
using EMI.EmployeeManagement.Entities.Dto;

namespace EMI.EmployeeManagement.BLL.Validators
{
    public static class UpdatePositionValidator
    {
        public static ValidationResult Validate(UpdatePositionRequest request)
        {
            var errors = new List<ValidationErrorItem>();

            if (request.EmployeeId <= 0)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidId,
                    Message = "Employee Id must be greater than zero."
                });
            }

            if (request.NewPositionId <= 0)
            {
                errors.Add(new ValidationErrorItem
                {
                    Code = EmployeeValidationErrorEnum.InvalidCurrentPositionId,
                    Message = "Invalid new position id."
                });
            }

            if (errors.Any())
                return ValidationResult.Failure(errors.ToArray());

            return ValidationResult.Success();
        }
    }
}
