using MicroResult;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var users = new Dictionary<int, User>
{
	[1] = new(1, "Ava", "ava@example.com", 24, true),
	[2] = new(2, "NoEmail", "", 30, true),
	[3] = new(3, "Teen", "teen@example.com", 16, true),
	[4] = new(4, "Inactive", "inactive@example.com", 35, false)
};

app.MapGet("/", () => Results.Ok(new
{
	package = "MicroResult",
	endpoints = new[]
	{
		"GET /users/1",
		"GET /users/99",
		"GET /users/1/dto",
		"GET /users/2/dto",
		"GET /users/3/dto",
		"GET /users/4/dto"
	}
}));

app.MapGet("/users/{id:int}", (int id) =>
{
	var result = GetUser(id, users);

	return result.Match<IResult>(
		user => Results.Ok(new
		{
			status = "success",
			value = user
		}),
		error => Results.NotFound(new
		{
			status = "failure",
			error = new { error.Code, error.Message }
		}));
});

app.MapGet("/users/{id:int}/dto", (int id) =>
{
	var result = GetUser(id, users)
		.Bind(EnsureActive)
		.Bind(EnsureAdult)
		.Ensure(user => !string.IsNullOrWhiteSpace(user.Email), Errors.MissingEmail)
		.Tap(user => app.Logger.LogInformation("Mapped user {UserId}", user.Id))
		.Map(user => new UserDto(user.Id, user.Name, user.Email));

	return result.Match<IResult>(
		dto => Results.Ok(new
		{
			status = "success",
			value = dto
		}),
		error => Results.BadRequest(new
		{
			status = "failure",
			error = new { error.Code, error.Message }
		}));
});

app.Run();

static Result<User> GetUser(int id, IReadOnlyDictionary<int, User> users)
{
	return users.TryGetValue(id, out var user)
		? user
		: Errors.NotFound;
}

static Result<User> EnsureActive(User user)
{
	return user.IsActive
		? user
		: Errors.Inactive;
}

static Result<User> EnsureAdult(User user)
{
	return user.Age >= 18
		? user
		: Errors.UnderAge;
}

static class Errors
{
	public static readonly Error NotFound = new("NotFound", "User not found.");
	public static readonly Error Inactive = new("Inactive", "User is inactive.");
	public static readonly Error UnderAge = new("UnderAge", "User must be at least 18.");
	public static readonly Error MissingEmail = new("MissingEmail", "User email is required.");
}

internal sealed record User(int Id, string Name, string Email, int Age, bool IsActive);

internal sealed record UserDto(int Id, string Name, string Email);
