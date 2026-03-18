using System;
using Microsoft.AspNetCore.Http;

namespace MicroResult;

/// <summary>
/// ASP.NET Core integration for Result{T}.
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Converts a Result{T} to an IResult for use in Minimal APIs.
    /// Success returns Ok(value), Failure returns BadRequest(error).
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.Match(
            onSuccess: value => Results.Ok(value),
            onFailure: error => Results.BadRequest(new { error.Code, error.Message })
        );
    }

    /// <summary>
    /// Converts a Result{T} to an IResult with custom handlers.
    /// </summary>
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, IResult> onSuccess,
        Func<Error, IResult> onFailure)
    {
        return result.Match(onSuccess, onFailure);
    }
}
