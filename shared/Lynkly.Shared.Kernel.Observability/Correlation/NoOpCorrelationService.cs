namespace Lynkly.Shared.Kernel.Observability.Correlation;

internal sealed class NoOpCorrelationService : ICorrelationService
{
    public string GetOrCreateCorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }
}
