using System;
using Xunit;
using MicroResult;

namespace MicroResult.Tests;

public class ErrorTests
{
    [Fact]
    public void Error_Constructor_SetsProperties()
    {
        var error = new Error("TestCode", "Test message");

        Assert.Equal("TestCode", error.Code);
        Assert.Equal("Test message", error.Message);
    }

    [Fact]
    public void Error_ToString_FormatsCorrectly()
    {
        var error = new Error("NotFound", "User not found");

        Assert.Equal("NotFound: User not found", error.ToString());
    }

    [Fact]
    public void Error_Equality_WorksCorrectly()
    {
        var error1 = new Error("Code1", "Message1");
        var error2 = new Error("Code1", "Message1");
        var error3 = new Error("Code2", "Message2");

        Assert.Equal(error1, error2);
        Assert.NotEqual(error1, error3);
        Assert.True(error1 == error2);
        Assert.True(error1 != error3);
    }
}

public class ResultSuccessTests
{
    [Fact]
    public void Success_IsSuccess_ReturnsTrue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Success_Value_ReturnsValue()
    {
        var result = Result<int>.Success(42);

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Success_Error_ThrowsException()
    {
        var result = Result<int>.Success(42);

        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Success_ImplicitConversion_FromValue()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }
}

public class ResultFailureTests
{
    [Fact]
    public void Failure_IsFailure_ReturnsTrue()
    {
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Failure_Error_ReturnsError()
    {
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);

        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_Value_ThrowsException()
    {
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_ImplicitConversion_FromError()
    {
        var error = new Error("TestCode", "Test message");
        Result<int> result = error;

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }
}

public class ResultMatchTests
{
    [Fact]
    public void Match_OnSuccess_CallsSuccessHandler()
    {
        var result = Result<int>.Success(42);
        var output = result.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: e => $"Failure: {e.Code}"
        );

        Assert.Equal("Success: 42", output);
    }

    [Fact]
    public void Match_OnFailure_CallsFailureHandler()
    {
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);
        var output = result.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: e => $"Failure: {e.Code}"
        );

        Assert.Equal("Failure: TestCode", output);
    }
}

public class ResultMapTests
{
    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Success(5);
        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_PreservesError()
    {
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);
        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsFailure);
        Assert.Equal(error, mapped.Error);
    }

    [Fact]
    public void Map_CanChangeType()
    {
        var result = Result<int>.Success(42);
        var mapped = result.Map(x => x.ToString());

        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }
}

public class ResultBindTests
{
    private Result<int> DoubleIfPositive(int x) =>
        x > 0 ? x * 2 : new Error("Negative", "Value must be positive");

    [Fact]
    public void Bind_OnSuccess_ChainsOperations()
    {
        var result = Result<int>.Success(5);
        var bound = result.Bind(DoubleIfPositive);

        Assert.True(bound.IsSuccess);
        Assert.Equal(10, bound.Value);
    }

    [Fact]
    public void Bind_OnFailure_PreservesError()
    {
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);
        var bound = result.Bind(DoubleIfPositive);

        Assert.True(bound.IsFailure);
        Assert.Equal(error, bound.Error);
    }

    [Fact]
    public void Bind_PropagatesFailureFromChain()
    {
        var result = Result<int>.Success(-5);
        var bound = result.Bind(DoubleIfPositive);

        Assert.True(bound.IsFailure);
        Assert.Equal("Negative", bound.Error.Code);
    }
}

public class ResultEnsureTests
{
    [Fact]
    public void Ensure_PredicateTrue_PreservesResult()
    {
        var result = Result<int>.Success(42);
        var ensured = result.Ensure(x => x > 0, new Error("Invalid", "Must be positive"));

        Assert.True(ensured.IsSuccess);
        Assert.Equal(42, ensured.Value);
    }

    [Fact]
    public void Ensure_PredicateFalse_ReturnsError()
    {
        var error = new Error("Negative", "Must be positive");
        var result = Result<int>.Success(-5);
        var ensured = result.Ensure(x => x > 0, error);

        Assert.True(ensured.IsFailure);
        Assert.Equal(error, ensured.Error);
    }

    [Fact]
    public void Ensure_OnFailure_PreservesError()
    {
        var initialError = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(initialError);
        var ensured = result.Ensure(x => x > 0, new Error("Other", "Other error"));

        Assert.True(ensured.IsFailure);
        Assert.Equal(initialError, ensured.Error);
    }
}

public class ResultExtensionTests
{
    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        var called = false;
        var result = Result<int>.Success(42);
        var tapped = result.Tap(_ => called = true);

        Assert.True(called);
        Assert.True(tapped.IsSuccess);
        Assert.Equal(42, tapped.Value);
    }

    [Fact]
    public void Tap_OnFailure_SkipsAction()
    {
        var called = false;
        var result = Result<int>.Failure(new Error("Code", "Message"));
        var tapped = result.Tap(_ => called = true);

        Assert.False(called);
        Assert.True(tapped.IsFailure);
    }

    [Fact]
    public void OnFailure_OnSuccess_SkipsAction()
    {
        var called = false;
        var result = Result<int>.Success(42);
        var handled = result.OnFailure(_ => called = true);

        Assert.False(called);
        Assert.True(handled.IsSuccess);
    }

    [Fact]
    public void OnFailure_OnFailure_ExecutesAction()
    {
        var error = new Error("Code", "Message");
        var called = false;
        var result = Result<int>.Failure(error);
        var handled = result.OnFailure(_ => called = true);

        Assert.True(called);
        Assert.True(handled.IsFailure);
    }
}

public class ResultChainTests
{
    [Fact]
    public void ChainMapAndBind_ComplexFlow()
    {
        var result = Result<int>.Success(5)
            .Map(x => x * 2)
            .Bind(x => x > 5 ? Result<int>.Success(x + 10) : new Error("TooSmall", "Too small"))
            .Map(x => x.ToString());

        Assert.True(result.IsSuccess);
        Assert.Equal("20", result.Value);
    }

    [Fact]
    public void ChainWithEnsure_StopsOnFailure()
    {
        var result = Result<int>.Success(3)
            .Ensure(x => x > 5, new Error("TooSmall", "Must be greater than 5"))
            .Map(x => x * 2);

        Assert.True(result.IsFailure);
        Assert.Equal("TooSmall", result.Error.Code);
    }
}
