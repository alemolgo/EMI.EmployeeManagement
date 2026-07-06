using EMI.EmployeeManagement.Common.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMI.EmployeeManagement.Common.Validations
{
    public class ValidationErrorItem
    {
        public EmployeeValidationErrorEnum Code { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
