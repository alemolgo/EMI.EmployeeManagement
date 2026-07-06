namespace EMI.EmployeeManagement.Entities.Dto
{
    public class AuthUserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
