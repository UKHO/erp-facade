using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class Result
    {
        public enum Statuses
        {
            Unknown = 0,
            Failure = 1,
            Success = 2,
            Retry = 3,
            Partial = 4,
        }

        [JsonConstructor]
        public Result(string message, Statuses status)
        {
            Message = message;
            Status = status;
        }

        public Result(Statuses status, [NotNull] string message)
        {
            Status = status;
            Message = message;
        }

        public string Message { get; }
        public Statuses Status { get; }

        public static Result Success(string message)
        {
            return new Result(Statuses.Success, message);
        }

        public static Result Failure(string message)
        {
            return new Result(Statuses.Failure, message);
        }

        public static Result Retry(string message)
        {
            return new Result(Statuses.Retry, message);
        }

        public static Result Partial(string message)
        {
            return new Result(Statuses.Partial, message);
        }
    }

    [ExcludeFromCodeCoverage]
    public class Result<T> : Result
    {
        [JsonConstructor]
        public Result(string message, Statuses status, T value) : base(message, status)
        {
            Value = value;
        }

        public Result(Statuses status, [NotNull] T value) : base(status, "")
        {
            Value = value;
        }

        public Result(Statuses status, [NotNull] string message, T value) : base(status, message)
        {
            Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(Statuses.Success, "", value);
        }

        public static Result<T> Success(string message, T value)
        {
            return new Result<T>(Statuses.Success, message, value);
        }

        public static Result<T> Failure(string message, T value)
        {
            return new Result<T>(Statuses.Failure, message, value);
        }

        public static Result<T> Retry(string message, T value)
        {
            return new Result<T>(Statuses.Retry, message, value);
        }

        public T Value { get; set; }
    }
}
