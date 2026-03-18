using System;

namespace MicroResult;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public readonly struct Error
{
    /// <summary>
    /// Gets the machine-readable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new error value.
    /// </summary>
    /// <param name="code">The machine-readable error code.</param>
    /// <param name="message">The human-readable error message.</param>
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Returns the error as a readable string.
    /// </summary>
    public override string ToString() => $"{Code}: {Message}";

    /// <summary>
    /// Determines whether this error equals another object.
    /// </summary>
    public override bool Equals(object? obj) => obj is Error error && Code == error.Code && Message == error.Message;
    
    /// <summary>
    /// Returns a hash code for this error.
    /// </summary>
    public override int GetHashCode()
    {
#if NET6_0_OR_GREATER
        return HashCode.Combine(Code, Message);
#else
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (Code?.GetHashCode() ?? 0);
            hash = hash * 31 + (Message?.GetHashCode() ?? 0);
            return hash;
        }
#endif
    }

    /// <summary>
    /// Compares two errors for equality.
    /// </summary>
    public static bool operator ==(Error left, Error right) => left.Equals(right);

    /// <summary>
    /// Compares two errors for inequality.
    /// </summary>
    public static bool operator !=(Error left, Error right) => !left.Equals(right);
}
