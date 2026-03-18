# MicroResult Quick Reference

## Core Types

```csharp
// Error struct
public readonly struct Error
{
    public string Code { get; }
    public string Message { get; }
    public Error(string code, string message);
}

// Result<T> struct
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T Value { get; }              // ⚠️ Throws if IsFailure
    public Error Error { get; }          // ⚠️ Throws if IsSuccess
}
```

## Creating Results

```csharp
// Explicit factory methods
Result<int> success = Result<int>.Success(42);
Result<int> failure = Result<int>.Failure(new Error("Code", "Message"));

// Implicit conversions (recommended)
Result<int> fromValue = 42;                           // Success implicit
Result<int> fromError = new Error("Code", "Msg");     // Failure implicit

// From your error constants
Result<User> notFound = Errors.NotFound;
```

## Checking Results

```csharp
if (result.IsSuccess)
{
    var value = result.Value;
}

if (result.IsFailure)
{
    var error = result.Error;
    var code = error.Code;
    var message = error.Message;
}
```

## Pattern Matching

```csharp
// Match pattern
result.Match(
    onSuccess: value => ... ,
    onFailure: error => ...
);

// With return
string message = result.Match(
    value => $"Success: {value}",
    error => $"Error: {error.Code}"
);

// As IResult (Minimal APIs)
return result.ToHttpResult();
```

## Transforming Results

```csharp
// Map: Transform the value (success → success, failure → failure)
Result<int> squared = result.Map(x => x * x);

// Bind: Chain operations returning Result
Result<User> user = GetUser(id).Bind(ValidateUser);

// Ensure: Add validation predicates
result.Ensure(x => x > 0, Errors.MustBePositive);
```

## Side Effects

```csharp
// Tap: Execute action on success, return same result
result
    .Tap(value => Console.WriteLine($"Got: {value}"))
    .Map(x => x * 2);

// OnFailure: Execute action on failure, return same result
result
    .OnFailure(error => Log.Error(error.Message))
    .Map(x => x * 2);
```

## Chaining Examples

### Simple Chain
```csharp
GetUser(id)
    .Map(u => u.Email)
    .Match(
        email => Ok(email),
        error => BadRequest(error)
    );
```

### Multiple Operations
```csharp
GetUser(id)
    .Bind(user => AuthorizeUser(user))    // Chain Results
    .Map(user => new UserDto(user))       // Transform value
    .Ensure(dto => !dto.IsDeleted, Errors.UserDeleted)  // Validate
    .Tap(dto => _logger.LogInformation($"Got user: {dto.Id}"))  // Log
    .Match(
        dto => Ok(dto),
        error => BadRequest(error)
    );
```

### Validation Chain
```csharp
Result<Order>.Success(order)
    .Ensure(o => o.Items.Count > 0, Errors.EmptyOrder)
    .Ensure(o => o.Total > 0, Errors.InvalidPrice)
    .Ensure(o => o.CustomerId != Guid.Empty, Errors.NoCustomer)
    .Map(o => CreateOrderDto(o));
```

## ASP.NET Integration

### Minimal API
```csharp
// Auto-convert Result to IResult
app.MapGet("/users/{id}", (Guid id, UserService svc) =>
    svc.GetUser(id)
        .Map(u => new UserDto(u))
        .ToHttpResult()  // Success → 200 Ok, Failure → 400 BadRequest
);

// Custom response mappings
app.MapPost("/users", (CreateUserRequest req, UserService svc) =>
{
    var result = svc.CreateUser(req);
    
    return result.ToHttpResult(
        onSuccess: user => Results.Created($"/users/{user.Id}", user),
        onFailure: error => error.Code switch
        {
            "DuplicateEmail" => Results.Conflict(error),
            "Unauthorized" => Results.Forbid(),
            _ => Results.BadRequest(error)
        }
    );
});
```

### Error Constants Pattern
```csharp
public static class Errors
{
    public static readonly Error NotFound = new("NotFound", "Resource not found");
    public static readonly Error Unauthorized = new("Unauthorized", "Not authorized");
    public static readonly Error ValidationFailed = new("ValidationFailed", "Validation failed");
    public static readonly Error DuplicateEmail = new("DuplicateEmail", "Email already exists");
}

// Usage
return Errors.NotFound;               // Clean, type-safe
return new Error("Invalid", "msg");   // Or explicit
```

## Common Patterns

### Try-Like Pattern
```csharp
public Result<T> TryGetValue<T>(string key)
{
    if (dictionary.TryGetValue(key, out var value))
        return value;
    
    return Errors.KeyNotFound;
}
```

### Null-Checking Pattern
```csharp
public Result<User> GetUser(Guid id)
{
    var user = db.Users.Find(id);
    return user != null ? user : Errors.NotFound;
}
```

### Conditional Chain
```csharp
public Result<UserDto> GetUserIfAdmin(Guid userId, Guid requestedId)
{
    return GetUser(userId)
        .Ensure(u => u.IsAdmin, Errors.NotAdmin)
        .Bind(_ => GetUser(requestedId))
        .Map(u => new UserDto(u));
}
```

### Accumulate Errors (multiple validations)
```csharp
var validationResult = Result<User>.Success(user)
    .Ensure(u => !string.IsNullOrEmpty(u.Email), Errors.NoEmail)
    .Ensure(u => u.Email.Contains("@"), Errors.InvalidEmail)
    .Ensure(u => u.Age >= 18, Errors.UnderAge);

// If any Ensure fails, subsequent chains preserve that error
```

## Performance Tips

1. **Use Stack**: Result<T> is a value type - it lives on the stack
2. **No Allocation**: No GC pressure in happy path
3. **No Exception Overhead**: Errors are values, not exceptions
4. **Use Implicit Conversions**: More idiomatic, same performance
5. **Avoid `.Value`**: Use `.Match()` or check `.IsSuccess` first

## Common Mistakes

### ❌ Accessing .Value without checking
```csharp
var value = result.Value;  // Throws InvalidOperationException!
```

### ✅ Correct
```csharp
var value = result.IsSuccess ? result.Value : default!;
// Or better:
result.Match(
    v => v,
    e => default!
);
```

### ❌ Swallowing errors
```csharp
var user = GetUser(id).Match(
    u => u,
    _ => null  // ⚠️ Lost error information
);
```

### ✅ Handle them
```csharp
result.Match(
    user => ... ,
    error => Log.Error(error.Message)  // Track the error
);
```

### ❌ Forgetting the chain continues on failure
```csharp
GetUser(id)
    .Map(u => u.Email)
    .Map(e => e.ToUpper())  // ⚠️ Still maps on failure!
```

### ✅ It actually works (preserves error)
```csharp
// All Maps after a failure are skipped, error is preserved
GetUser(id)
    .Map(u => u.Email)      // Failure → skip
    .Map(e => e.ToUpper())  // Failure → skip
    .Match(
        success => success,
        error => error      // ✅ Original error
    );
```

## Testing Patterns

```csharp
[Fact]
public void GetUser_WithValidId_ReturnsSuccess()
{
    var result = service.GetUser(userId);
    
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedUser.Id, result.Value.Id);
}

[Fact]
public void GetUser_WithInvalidId_ReturnsNotFound()
{
    var result = service.GetUser(Guid.NewGuid());
    
    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
}

[Fact]
public void Chain_PreservesErrorThroughChain()
{
    var result = GetUser(invalidId)
        .Map(u => u.Email)
        .Bind(ValidateEmail);
    
    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
}
```

---

**Pro Tip**: Chain operations early, match results late. This keeps error handling centralized and code clean.
