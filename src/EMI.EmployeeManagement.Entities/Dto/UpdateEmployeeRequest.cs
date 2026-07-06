using System.Text.Json.Serialization;

namespace EMI.EmployeeManagement.Entities.Dto
{
    public class UpdateEmployeeRequest
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string? Name { get;  set; }
        public decimal? Salary { get;  set; }
    }
}
