namespace Lynkly.Shared.Kernel.Context;

/// <summary>
/// Provides an ambient, <see cref="System.Threading.AsyncLocal{T}"/>-based scope
/// for propagating an <see cref="AppContext"/> through asynchronous call stacks
/// without explicit parameter passing.
/// </summary>
public static class RequestContextScope
{
    private static readonly AsyncLocal<AppContext?> CurrentContext = new();

    /// <summary>
    /// Gets the <see cref="AppContext"/> ambient for the current asynchronous execution context,
    /// or <see langword="null"/> if no scope has been established.
    /// </summary>
    public static AppContext? Current => CurrentContext.Value;

    /// <summary>
    /// Establishes <paramref name="appContext"/> as the ambient context for the current
    /// asynchronous execution context.
    /// </summary>
    /// <param name="appContext">The context to make ambient. Must not be <see langword="null"/>.</param>
    /// <returns>
    /// A disposable that, when disposed, restores the context that was ambient before this call.
    /// Double-disposing is safe and has no effect beyond the first call.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="appContext"/> is <see langword="null"/>.
    /// </exception>
    public static IDisposable BeginScope(AppContext appContext)
    {
        ArgumentNullException.ThrowIfNull(appContext);

        var previousContext = CurrentContext.Value;
        CurrentContext.Value = appContext;

        return new Scope(previousContext);
    }

    private sealed class Scope(AppContext? previousContext) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            CurrentContext.Value = previousContext;
            _disposed = true;
        }
    }
}
