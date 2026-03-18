# MicroResult

The smallest, fastest, AOT-safe Result type for modern .NET. **Zero dependencies. Zero allocations.**

Perfect for Minimal APIs, Blazor, microservices, and any high-performance code paths.

## Features

✅ **Zero Dependencies** — Just add and use  
⚡ **Zero Allocations** — Readonly struct, stack-only  
🚀 **AOT Friendly** — Native AOT compilation ready  
🎯 **Minimal API** — Only essential methods (Match, Map, Bind, Ensure)  
🔗 **Fluent Chaining** — Composable, functional error handling  
✨ **Clean DX** — Implicit conversions + Match patterns  
🌐 **ASP.NET Integration** — Seamless Minimal API support

## Installation

```bash
dotnet add package MicroResult
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

### ASP.NET Minimal API

```csharp
app.MapGet("/users/{id}", (Guid id, UserService service) =>
{
    var result = service.GetUser(id);
    return result.ToHttpResult();
});
```

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

// LINQ-like (Select, SelectMany)
public static Result<TResult> Select<T, TResult>(this Result<T> result, Func<T, TResult> selector);
public static Result<TResult> SelectMany<T, TResult>(this Result<T> result, Func<T, Result<TResult>> selector);
```

### ASP.NET Integration

```csharp
// Standard: Success → Ok(value), Failure → BadRequest(error)
public static IResult ToHttpResult<T>(this Result<T> result);

// Custom handlers
public static IResult ToHttpResult<T>(
    this Result<T> result,
    Func<T, IResult> onSuccess,
    Func<Error, IResult> onFailure);
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

### Minimal API Handler

```csharp
app.MapPost("/users", async (CreateUserRequest req, UserService service) =>
{
    var result = await service.CreateUser(req);
    
    return result.ToHttpResult(
        onSuccess: user => Results.Created($"/users/{user.Id}", user),
        onFailure: error => error.Code switch
        {
            "Duplicate" => Results.Conflict(error),
            "Unauthorized" => Results.Forbid(),
            _ => Results.BadRequest(error)
        }
    );
});
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

## License

MIT — Use freely in personal and commercial projects.

## Contributing

Issues and PRs welcome on [GitHub](https://github.com/microresult/microresult).

---

**MicroResult:** The Result type you won't outgrow. 🚀
