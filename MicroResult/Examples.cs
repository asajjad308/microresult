using System;
using System.Collections.Generic;
using System.Linq;
using MicroResult;

namespace MicroResult.Examples;

// Example error definitions
public static class Errors
{
    public static readonly Error NotFound = new("NotFound", "Resource not found");
    public static readonly Error ValidationFailed = new("ValidationFailed", "Validation failed");
    public static readonly Error Unauthorized = new("Unauthorized", "User is not authorized");
    public static readonly Error InternalError = new("InternalError", "An internal error occurred");
}

// Example domain model
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// Example service
public class UserService
{
    private readonly Dictionary<Guid, User> _users = new();

    public UserService()
    {
        _users[Guid.NewGuid()] = new User { Id = Guid.NewGuid(), Email = "john@example.com", Name = "John Doe" };
    }

    /// <summary>
    /// Get user by ID - returns Result{User}.
    /// </summary>
    public Result<User> GetUser(Guid id)
    {
        var user = _users.Values.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
            return Errors.NotFound;

        return user;
    }

    /// <summary>
    /// Validate user logic
    /// </summary>
    public Result<User> ValidateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            return Errors.ValidationFailed;

        return user;
    }

    /// <summary>
    /// Example: Chaining operations with Bind and Map
    /// </summary>
    public Result<UserDto> GetUserDto(Guid id)
    {
        return GetUser(id)
            .Bind(ValidateUser)
            .Map(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            });
    }

    /// <summary>
    /// Example: Using Match for handling results
    /// </summary>
    public string GetUserSummary(Guid id)
    {
        return GetUser(id).Match(
            onSuccess: user => $"User: {user.Name} ({user.Email})",
            onFailure: error => $"Error: {error.Code} - {error.Message}"
        );
    }

    /// <summary>
    /// Example: Implicit conversions
    /// </summary>
    public Result<User> GetUserImplicitConversion(Guid id)
    {
        var user = _users.Values.FirstOrDefault(u => u.Id == id);
        
        // Implicit conversion from User to Result<User>
        if (user == null)
            return Errors.NotFound; // Implicit conversion from Error to Result<User>

        return user;
    }

    /// <summary>
    /// Example: Using Ensure for validation
    /// </summary>
    public Result<User> GetVerifiedUser(Guid id)
    {
        return GetUser(id)
            .Ensure(u => !string.IsNullOrEmpty(u.Email), new Error("InvalidEmail", "User email is missing"));
    }

    /// <summary>
    /// Example: Using Tap for side effects
    /// </summary>
    public Result<User> GetUserWithLogging(Guid id)
    {
        return GetUser(id)
            .Tap(user => Console.WriteLine($"Retrieved user: {user.Name}"))
            .OnFailure(error => Console.WriteLine($"Failed to get user: {error}"));
    }
}

// Example ASP.NET Minimal API usage (conceptual)
/*
var app = WebApplication.CreateBuilder(args).Build();

var userService = new UserService();

app.MapGet("/users/{id}", async (Guid id) =>
{
    var result = userService.GetUserDto(id);
    return result.ToHttpResult();
});

app.MapPost("/users/{id}/verify", async (Guid id) =>
{
    var result = userService.GetVerifiedUser(id);
    
    // Custom handlers
    return result.ToHttpResult(
        onSuccess: user => Results.Ok(new { message = "User verified", user }),
        onFailure: error => Results.BadRequest(error)
    );
});

app.Run();
*/
