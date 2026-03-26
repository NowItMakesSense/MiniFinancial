namespace MiniFinancial.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IReadOnlyList<string> Errors { get; }

        protected Result(bool isSuccess, IEnumerable<string> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors?.ToList() ?? new List<string>();
        }

        public static Result Success() => new(true, Array.Empty<string>());
        public static Result Failure(params string[] errors) => new(false, errors);
        public static Result Failure(IEnumerable<string> errors) => new(false, errors);

        // Async helpers
        public static Task<Result> SuccessAsync() => Task.FromResult(Success());
        public static Task<Result> FailureAsync(params string[] errors) => Task.FromResult(Failure(errors));
        public static Task<Result> FailureAsync(IEnumerable<string> errors) => Task.FromResult(Failure(errors));
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(bool isSuccess, T? value, IEnumerable<string> errors) : base(isSuccess, errors)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());
        public static new Result<T> Failure(params string[] errors) => new(false, default, errors);
        public static new Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);

        public static Task<Result<T>> SuccessAsync(T value) => Task.FromResult(Success(value));
        public static new Task<Result<T>> FailureAsync(params string[] errors) => Task.FromResult(Failure(errors));
        public static new Task<Result<T>> FailureAsync(IEnumerable<string> errors) => Task.FromResult(Failure(errors));

        public Result<TResult> Map<TResult>(Func<T, TResult> func)
        {
            return IsSuccess && Value != null
                ? Result<TResult>.Success(func(Value))
                : Result<TResult>.Failure(Errors);
        }
    }
}
