namespace EMI.EmployeeManagement.Common.Exceptions
{
    public class DaoException : System.Exception
    {
        private readonly string? message;
        private readonly System.Exception exception;

        public DaoException() : base() { }

        public DaoException(string _message) : base(_message)
        {
            message = _message;
        }

        public DaoException(string _message, System.Exception _exception) : base(_message, _exception)
        {
            message = _message;
            exception = _exception;
        }
    }
}
