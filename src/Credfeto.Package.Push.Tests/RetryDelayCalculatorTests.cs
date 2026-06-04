using System;
using Credfeto.Package.Push.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Package.Push.Tests;

public sealed class RetryDelayCalculatorTests : TestBase
{
    private static readonly TimeSpan MinDelay = TimeSpan.FromSeconds(5);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void FirstAttemptAlwaysReturnsMinDelay(int attempts)
    {
        TimeSpan result = RetryDelayCalculator.CalculateDelay(
            attempts: attempts,
            maxJitterSeconds: 5,
            randomFraction: 0.5
        );
        Assert.Equal(MinDelay, result);
    }

    [Theory]
    [InlineData(2, 0.0, 7)]
    [InlineData(2, 1.0, 11)]
    [InlineData(3, 0.0, 8)]
    [InlineData(3, 1.0, 18)]
    [InlineData(4, 0.0, 16)]
    [InlineData(4, 1.0, 26)]
    public void SubsequentAttemptsReturnExponentialBackoffWithJitter(
        int attempts,
        double randomFraction,
        int expectedSeconds
    )
    {
        TimeSpan result = RetryDelayCalculator.CalculateDelay(
            attempts: attempts,
            maxJitterSeconds: 5,
            randomFraction: randomFraction
        );
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result);
    }

    [Fact]
    public void DelayIncreasesWithMoreAttempts()
    {
        TimeSpan delay2 = RetryDelayCalculator.CalculateDelay(attempts: 2, maxJitterSeconds: 5, randomFraction: 0.5);
        TimeSpan delay3 = RetryDelayCalculator.CalculateDelay(attempts: 3, maxJitterSeconds: 5, randomFraction: 0.5);
        TimeSpan delay4 = RetryDelayCalculator.CalculateDelay(attempts: 4, maxJitterSeconds: 5, randomFraction: 0.5);

        Assert.True(delay2 < delay3, "Delay for attempt 3 should exceed delay for attempt 2");
        Assert.True(delay3 < delay4, "Delay for attempt 4 should exceed delay for attempt 3");
    }

    [Fact]
    public void DelayIsNeverLessThanMinDelay()
    {
        for (int attempts = 1; attempts <= 6; attempts++)
        {
            TimeSpan result = RetryDelayCalculator.CalculateDelay(
                attempts: attempts,
                maxJitterSeconds: 5,
                randomFraction: 0.0
            );
            Assert.True(
                result >= MinDelay,
                $"Delay for attempt {attempts} should not be less than MinDelay ({MinDelay})"
            );
        }
    }

    [Fact]
    public void CalculateWithJitterReturnsAtLeastMinDelay()
    {
        TimeSpan result = RetryDelayCalculator.CalculateWithJitter(attempts: 2, maxJitterSeconds: 10);

        Assert.True(result >= MinDelay, $"CalculateWithJitter result {result} should be at least {MinDelay}");
    }
}
