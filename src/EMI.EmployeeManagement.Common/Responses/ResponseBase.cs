using EMI.EmployeeManagement.Common.Errors;
using EMI.EmployeeManagement.Common.Validations;

namespace EMI.EmployeeManagement.Common.Responses
{
    public class ResponseBase
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public ErrorTypeEnum ErrorCode { get; set; }

        public List<ValidationErrorItem>? ValidationErrors { get; set; }

        public List<string>? Errors { get; set; }
    }

}
