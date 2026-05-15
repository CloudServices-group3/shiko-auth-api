
/* 
 * This record is of generic type and is used as a "wrapper" to standardize service responses.
 * It represents either a successful result with a value of type T,
 * or a failed result with error information.
 * 
 * This avoids the need for separate DTOs for success/failure states and reduces the use of nullable properties to represent different outcomes.
 */
namespace Application.Common;

public sealed record Result<T>
(
    bool isSuccess,
    T? Value,
    string? ErrorMessage,
    Dictionary<string, string[]>? errors = null
)
{
    public static Result<T> Ok(T value)
        => new(true, value, null);

    public static Result<T> Fail(string message)
        => new(false, default, message);

    public static Result<T> Fail(Dictionary<string, string[]> errors)
        => new(false, default, null, errors);
};
