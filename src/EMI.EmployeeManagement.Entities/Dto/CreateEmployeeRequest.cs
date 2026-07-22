namespace EMI.EmployeeManagement.Entities.Dto
{
    public class CreateEmployeeRequest
    {
        public string? Name { get; set; }
        public int CurrentPositionId { get; set; }
        public decimal Salary { get; set; }
        public string? Password { get; set; }
        public int RoleId { get; set; }
    }
}
