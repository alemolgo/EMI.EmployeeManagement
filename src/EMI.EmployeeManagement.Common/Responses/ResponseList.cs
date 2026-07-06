namespace EMI.EmployeeManagement.Common.Responses
{
    public class ResponseList<T> : ResponseBase
    {
        public IEnumerable<T>? Data { get; set; }
    }
}
