using System;
using Credfeto.Package.Push.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Package.Push.Tests.Exceptions;

public sealed class UploadConfigurationErrorsExceptionTests : TestBase
{
    [Fact]
    public void DefaultConstructorProducesExpectedMessage()
    {
        UploadConfigurationErrorsException exception = new();

        Assert.Equal(expected: "Configuration errors were encountered.", actual: exception.Message);
    }

    [Fact]
    public void MessageConstructorProducesExpectedMessage()
    {
        const string MESSAGE = "Custom error message.";

        UploadConfigurationErrorsException exception = new(MESSAGE);

        Assert.Equal(expected: MESSAGE, actual: exception.Message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructorProducesExpectedValues()
    {
        const string MESSAGE = "Outer error message.";
        InvalidOperationException inner = new("Inner error.");

        UploadConfigurationErrorsException exception = new(message: MESSAGE, innerException: inner);

        Assert.Equal(expected: MESSAGE, actual: exception.Message);
        Assert.Same(expected: inner, actual: exception.InnerException);
    }
}
