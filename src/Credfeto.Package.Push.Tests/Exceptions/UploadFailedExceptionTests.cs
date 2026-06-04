using System;
using Credfeto.Package.Push.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Package.Push.Tests.Exceptions;

public sealed class UploadFailedExceptionTests : TestBase
{
    [Fact]
    public void DefaultConstructorProducesExpectedMessage()
    {
        UploadFailedException exception = new();

        Assert.Equal(expected: "Upload failed.", actual: exception.Message);
    }

    [Fact]
    public void MessageConstructorProducesExpectedMessage()
    {
        const string MESSAGE = "Custom upload failed message.";

        UploadFailedException exception = new(MESSAGE);

        Assert.Equal(expected: MESSAGE, actual: exception.Message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructorProducesExpectedValues()
    {
        const string MESSAGE = "Outer upload failed message.";
        InvalidOperationException inner = new("Inner error.");

        UploadFailedException exception = new(message: MESSAGE, innerException: inner);

        Assert.Equal(expected: MESSAGE, actual: exception.Message);
        Assert.Same(expected: inner, actual: exception.InnerException);
    }
}
