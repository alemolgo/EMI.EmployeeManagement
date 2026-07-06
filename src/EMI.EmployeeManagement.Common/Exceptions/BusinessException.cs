namespace EMI.EmployeeManagement.Common.Exceptions
{
    public abstract class BusinessException : System.Exception
    {
        public BusinessException(string message) : base(message) { }
        public virtual int Code { get; set; }
    }
}
