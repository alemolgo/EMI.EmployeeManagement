namespace EMI.EmployeeManagement.Common.Responses
{
    public class ResponseQuery<T> : ResponseBase
    {
        public T? Data { get; set; }
    }
}
