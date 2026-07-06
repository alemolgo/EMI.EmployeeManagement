namespace EMI.EmployeeManagement.Common.Exceptions
{
    public class ChangePositionException : BusinessException
    {
        public ChangePositionException(EmployeeHistoryExceptionEnum createEmployeeHistoryExceptionEnum)
              : base(Detail(createEmployeeHistoryExceptionEnum).Item2)
        {
            Code = Detail(createEmployeeHistoryExceptionEnum).Item1;
        }

        public static Tuple<int, string> Detail(EmployeeHistoryExceptionEnum createEmployeeHistoryExceptionEnum)
        {
            var code = (int)createEmployeeHistoryExceptionEnum;
            var detail = createEmployeeHistoryExceptionEnum switch
            {
                EmployeeHistoryExceptionEnum.InvalidEmployeeId => new Tuple<int, string>(code, "Position history: emplomployee id is not valid."),
                EmployeeHistoryExceptionEnum.InvalidPositionId => new Tuple<int, string>(code, "Position history: new position id not valid."),

                _ => new Tuple<int, string>(code, "Unknown error creating History position.")
            };
            return detail;
        }
    }



    public enum EmployeeHistoryExceptionEnum
    {
        InvalidEmployeeId = 2001,
        InvalidPositionId = 2002,
    }
}
