# MicroResult

![MicroResult banner](https://raw.githubusercontent.com/asajjad308/microresult/main/assets/microresult-banner.svg)

MicroResult is a tiny library for returning either a successful value or an error without throwing exceptions in normal control flow.

It is designed for modern .NET applications that want predictable flow, low overhead, and a minimal API surface.

## Features

✅ **Zero Dependencies** — Just add and use  
⚡ **Zero Allocations** — Readonly struct, stack-only  
🚀 **AOT Friendly** — Native AOT compilation ready  
🎯 **Minimal API Surface** — Only essential methods (Match, Map, Bind, Ensure)  
🔗 **Fluent Chaining** — Composable, functional error handling  
✨ **Clean DX** — Implicit conversions + Match patterns

## Installation

```bash
dotnet add package MicroResult
```

## What It Does

Instead of throwing exceptions for expected failures, MicroResult returns a value that is either:

- a success containing `T`
- a failure containing `Error`

That makes business flow explicit and easy to compose.

```csharp
public Result<User> GetUser(Guid id)
{
    var user = db.Users.Find(id);

    if (user is null)
        return new Error("NotFound", "User not found");

    return user;
}
```

You then handle both outcomes deliberately:

```csharp
var result = GetUser(id);

return result.Match(
    user => Results.Ok(user),
    error => Results.BadRequest(error));
```

## Quick Start

### Basic Usage

```csharp
using MicroResult;

// Define errors
public static class Errors
{
    public static readonly Error NotFound = new("NotFound", "User not found");
    public static readonly Error Invalid = new("Invalid", "Invalid user");
}

// Return Result<T>
public Result<User> GetUser(Guid id)
{
    var user = db.Users.Find(id);
    if (user == null)
        return Errors.NotFound;  // Implicit conversion!
    
    return user;  // Implicit conversion!
}

// Consume with Match
var result = GetUser(id);
return result.Match(
    onSuccess: user => Ok(user),
    onFailure: error => BadRequest(error)
);
```

### Chaining with Bind & Map

```csharp
var result = GetUser(id)
    .Bind(ValidateUser)
    .Map(user => new UserDto(user));

return result.Match(
    user => Ok(user),
    error => BadRequest(error)
);
```

### Why Chaining Helps

```csharp
var result = GetUser(id)
    .Bind(ValidateUser)
    .Map(user => new UserDto(user));
```

Each step only runs when the previous step succeeded. If any step returns an error, the chain stops and preserves that failure.

## Core API

### Error

```csharp
public readonly struct Error
{
    public string Code { get; }
    public string Message { get; }
    
    public Error(string code, string message);
    public override string ToString(); // "Code: Message"
}
```

### Result<T>

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T Value { get; }           // Throws if failure
    public Error Error { get; }       // Throws if success
    
    // Factory methods
    public static Result<T> Success(T value);
    public static Result<T> Failure(Error error);
    
    // Core methods
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure);
    public Result<TResult> Map<TResult>(Func<T, TResult> func);
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> func);
    public Result<T> Ensure(Func<T, bool> predicate, Error error);
    
    // Implicit conversions
    public static implicit operator Result<T>(T value);
    public static implicit operator Result<T>(Error error);
}
```

### Extensions

```csharp
// Side effects
public static Result<T> Tap<T>(this Result<T> result, Action<T> action);
public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action);
```

## Real-World Use Cases

```csharp
// Validation
return age >= 18
    ? user
    : new Error("InvalidAge", "Must be 18 or older");
```

```csharp
// Database lookup
return entity is null
    ? new Error("NotFound", "Entity not found")
    : entity;
```

```csharp
// API response mapping
return result.Match(
    value => Results.Ok(value),
    error => Results.BadRequest(error));
```

## Optional ASP.NET Core Integration

The published package keeps the core library dependency-free.

If you want a `ToHttpResult()` extension for Minimal APIs, add the optional `HttpExtensions.optional.cs` helper from the repository to your ASP.NET Core project and reference the required ASP.NET package there.

```csharp
return result.ToHttpResult();
```

## Examples

### Validation Pattern

```csharp
public Result<User> ValidateUser(User user)
{
    return Result<User>.Success(user)
        .Ensure(u => !string.IsNullOrEmpty(u.Email), Errors.InvalidEmail)
        .Ensure(u => u.Age >= 18, Errors.UnderAge)
        .Ensure(u => u.Email.Contains("@"), Errors.InvalidFormat);
}
```

### Railway-Oriented Programming

```csharp
public Result<UserDto> CreateUser(CreateUserRequest req)
{
    return ValidateRequest(req)
        .Bind(r => CheckEmailExists(r.Email))
        .Bind(r => HasPermission(r))
        .Map(r => new User { Email = r.Email, Name = r.Name })
        .Tap(user => _db.Users.Add(user))
        .Map(user => new UserDto(user));
}
```

## Performance Characteristics

- **Value Type**: Stack allocation only, no GC pressure
- **No Exceptions in Happy Path**: Functional error handling
- **AOT Compatible**: Full Native AOT support, no reflection

## Supported Frameworks

- `.NET 10`
- `.NET 8.0`
- `.NET Standard 2.0`

## Design Philosophy

**Brutally minimal.** Every line of code must earn its place.

- No async wrappers (use C# async/await natively)
- No advanced LINQ operators
- No abstract base classes
- No external dependencies
- No magic

## Feedback

Issues and PRs are welcome on [GitHub](https://github.com/asajjad308/microresult).

## License

MIT — Use freely in personal and commercial projects.

---

**MicroResult:** The Result type you won't outgrow. 🚀
