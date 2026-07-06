using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Validations;

namespace EMI.EmployeeManagement.Common.Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }

        public List<ValidationErrorItem> Errors { get; set; } = new();

        public static ValidationResult Success()
            => new() { IsValid = true };

        public static ValidationResult Failure(params ValidationErrorItem[] errors)
            => new()
            {
                IsValid = false,
                Errors = errors.ToList()
            };
    }
}