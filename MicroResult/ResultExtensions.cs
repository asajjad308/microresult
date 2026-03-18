using System;

namespace MicroResult;

/// <summary>
/// Extension methods for Result{T}.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes an action on the value if success, then returns the original result.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes an action on the error if failure, then returns the original result.
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);
        return result;
    }

    /// <summary>
    /// Transforms a Result{T} to a Result{TResult} using a transformation function.
    /// </summary>
    public static Result<TResult> Select<T, TResult>(this Result<T> result, Func<T, TResult> selector)
        => result.Map(selector);

    /// <summary>
    /// Chains Results together (monadic bind).
    /// </summary>
    public static Result<TResult> SelectMany<T, TResult>(this Result<T> result, Func<T, Result<TResult>> selector)
        => result.Bind(selector);
}
