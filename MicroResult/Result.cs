using System;
using System.Collections.Generic;

namespace MicroResult;

/// <summary>
/// A lightweight, zero-allocation Result type for functional error handling.
/// </summary>
public readonly struct Result<T>
{
    private readonly T _value;
    private readonly Error _error;
    private readonly bool _isSuccess;

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public T Value
    {
        get
        {
            if (!IsSuccess)
                throw new InvalidOperationException("Cannot access Value when result is failure.");
            return _value;
        }
    }

    public Error Error
    {
        get
        {
            if (IsSuccess)
                throw new InvalidOperationException("Cannot access Error when result is success.");
            return _error;
        }
    }

    private Result(T value, Error error, bool isSuccess)
    {
        _value = value;
        _error = error;
        _isSuccess = isSuccess;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static Result<T> Success(T value) => new(value, default, true);

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    public static Result<T> Failure(Error error) => new(default!, error, false);

    /// <summary>
    /// Implicit conversion from T to Result{T} (success).
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from Error to Result{T} (failure).
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    /// Matches on success or failure, returning a result of TResult type.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value) : onFailure(_error);
    }

    /// <summary>
    /// Transforms the value if success, preserves failure.
    /// </summary>
    public Result<TResult> Map<TResult>(Func<T, TResult> func)
    {
        if (!IsSuccess)
            return Result<TResult>.Failure(_error);

        return Result<TResult>.Success(func(_value));
    }

    /// <summary>
    /// Chains operations that return Result{TResult}.
    /// </summary>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> func)
    {
        if (!IsSuccess)
            return Result<TResult>.Failure(_error);

        return func(_value);
    }

    /// <summary>
    /// Applies a predicate check; if fails, returns the given error.
    /// </summary>
    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        if (!IsSuccess)
            return this;

        return predicate(_value) ? this : Failure(error);
    }

    public override bool Equals(object? obj) => obj is Result<T> result && Equals(result);

    private bool Equals(Result<T> other)
    {
        if (IsSuccess != other.IsSuccess)
            return false;

        return IsSuccess ? EqualityComparer<T>.Default.Equals(_value, other._value) : _error.Equals(other._error);
    }

    public override int GetHashCode()
    {
#if NET6_0_OR_GREATER
        return IsSuccess ? HashCode.Combine(_value) : HashCode.Combine(_error);
#else
        unchecked
        {
            if (IsSuccess)
            {
                return 17 * 31 + EqualityComparer<T>.Default.GetHashCode(_value);
            }
            else
            {
                return 17 * 31 + _error.GetHashCode();
            }
        }
#endif
    }

    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);
    public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);
}
