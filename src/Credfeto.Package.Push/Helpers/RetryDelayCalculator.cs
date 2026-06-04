using System;
using System.Security.Cryptography;

namespace Credfeto.Package.Push.Helpers;

public static class RetryDelayCalculator
{
    private static readonly TimeSpan MinDelay = TimeSpan.FromSeconds(5);

    public static TimeSpan CalculateWithJitter(int attempts, int maxJitterSeconds)
    {
        // do a fast first retry, then exponential backoff
        return CalculateDelay(attempts: attempts, maxJitterSeconds: maxJitterSeconds, randomFraction: GetRandom());
    }

    public static TimeSpan CalculateDelay(int attempts, int maxJitterSeconds, double randomFraction)
    {
        return attempts <= 1
            ? MinDelay
            : MinDelay
                + TimeSpan.FromSeconds(
                    WithJitter(CalculateBackoff(attempts), maxSeconds: maxJitterSeconds, randomFraction: randomFraction)
                );
    }

    private static double CalculateBackoff(int attempts)
    {
        return Math.Pow(x: 2, y: attempts);
    }

    private static double WithJitter(double delaySeconds, int maxSeconds, double randomFraction)
    {
        double nonJitterPeriod = delaySeconds - maxSeconds;
        double jitterRange = maxSeconds * 2;

        if (nonJitterPeriod < 0)
        {
            jitterRange = delaySeconds;
            nonJitterPeriod = delaySeconds / 2;
        }

        double jitter = jitterRange * randomFraction;

        return nonJitterPeriod + jitter;
    }

    private static double GetRandom()
    {
        using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        {
            Span<byte> rnd = stackalloc byte[sizeof(uint)];
            randomNumberGenerator.GetBytes(rnd);
            uint random = BitConverter.ToUInt32(value: rnd);

            return random / (double)uint.MaxValue;
        }
    }
}
