using Microsoft.Extensions.Logging;

namespace Lynkly.Shared.Kernel.Logging.Abstractions;

public sealed class NoOpStructuredLogger<TCategory> : IStructuredLogger<TCategory>
{
    public static NoOpStructuredLogger<TCategory> Instance { get; } = new();

    public IDisposable BeginScope(IReadOnlyDictionary<string, object?> properties)
    {
        return NoOpScope.Instance;
    }

    public void LogInformation(string messageTemplate, params object?[] propertyValues)
    {
    }

    public void LogWarning(string messageTemplate, params object?[] propertyValues)
    {
    }

    public void LogError(Exception exception, string messageTemplate, params object?[] propertyValues)
    {
    }

    public bool IsEnabled(LogLevel logLevel) => false;

    private sealed class NoOpScope : IDisposable
    {
        public static NoOpScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
