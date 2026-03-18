using System;

namespace MicroResult;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public readonly struct Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public override string ToString() => $"{Code}: {Message}";

    public override bool Equals(object? obj) => obj is Error error && Code == error.Code && Message == error.Message;
    public override int GetHashCode() => HashCode.Combine(Code, Message);

    public static bool operator ==(Error left, Error right) => left.Equals(right);
    public static bool operator !=(Error left, Error right) => !left.Equals(right);
}
