namespace MarketPOS.Application.Common;

/// <summary>
/// Retries an operation that failed with a transient error, using exponential
/// backoff. Intended for DB and network calls per the Phase 1 error-handling rule
/// (max 3 attempts, exponential backoff).
/// </summary>
public static class TransientRetry
{
    /// <summary>
    /// Executes <paramref name="action"/>, retrying when <paramref name="isTransient"/>
    /// classifies the thrown exception as transient. The last failure is rethrown.
    /// </summary>
    /// <typeparam name="T">Result type of the operation.</typeparam>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <param name="isTransient">Classifier deciding whether an exception is transient.</param>
    /// <param name="maxAttempts">Total attempt count, including the first one.</param>
    /// <param name="cancellationToken">Cancels the backoff delay between attempts.</param>
    /// <returns>The result of the first successful attempt.</returns>
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        Func<Exception, bool> isTransient,
        int maxAttempts = 3,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < maxAttempts && isTransient(ex))
            {
                // 200ms, 400ms, 800ms, ... — cheap backoff without an extra dependency.
                var delay = TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
