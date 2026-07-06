using EMI.EmployeeManagement.Common.Exceptions;
using System.Text.Json.Serialization;

namespace EMI.EmployeeManagement.Entities.Dto
{
    public class UpdatePositionRequest
    {
        [JsonIgnore]
        public int EmployeeId { get; set; }
        public int NewPositionId { get; set; }

    }
}
