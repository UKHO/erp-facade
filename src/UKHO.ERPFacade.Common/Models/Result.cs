using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }

        public Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success()
        {
            return new Result(true, string.Empty);
        }

        public static Result Failure(string error)
        {
            return new Result(false, error);
        }
    }
}
