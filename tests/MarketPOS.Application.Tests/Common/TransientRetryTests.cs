using MarketPOS.Application.Common;

namespace MarketPOS.Application.Tests.Common;

/// <summary>
/// Tests for the exponential-backoff retry helper used around transient DB failures.
/// </summary>
public class TransientRetryTests
{
    [Fact]
    public async Task ExecuteAsync_RetriesTransientFailures_UntilSuccess()
    {
        var attempts = 0;

        var result = await TransientRetry.ExecuteAsync(
            () => ++attempts < 3 ? throw new TimeoutException() : Task.FromResult(42),
            ex => ex is TimeoutException);

        Assert.Equal(42, result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotRetry_NonTransientFailures()
    {
        var attempts = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TransientRetry.ExecuteAsync<int>(
                () => { attempts++; throw new InvalidOperationException(); },
                ex => ex is TimeoutException));

        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_StopsAfterMaxAttempts()
    {
        var attempts = 0;

        await Assert.ThrowsAsync<TimeoutException>(() =>
            TransientRetry.ExecuteAsync<int>(
                () => { attempts++; throw new TimeoutException(); },
                ex => ex is TimeoutException,
                maxAttempts: 3));

        Assert.Equal(3, attempts);
    }
}
