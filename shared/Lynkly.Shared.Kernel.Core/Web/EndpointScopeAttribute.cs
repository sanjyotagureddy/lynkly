namespace Lynkly.Shared.Kernel.Core.Web;

public enum EndpointScope
{
    Development,
    Uat,
    Production
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EndpointScopeAttribute : Attribute
{
    private readonly HashSet<EndpointScope> _allowedScopes;

    public EndpointScopeAttribute(params EndpointScope[] scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);

        if (scopes.Length == 0)
        {
            throw new ArgumentException("At least one endpoint scope must be specified.", nameof(scopes));
        }

        this._allowedScopes = scopes.ToHashSet();
    }

    public bool Includes(EndpointScope scope)
    {
        return this._allowedScopes.Contains(scope);
    }
}

public static class EndpointScopeResolver
{
    public static EndpointScope Resolve(string? environmentName)
    {
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            return EndpointScope.Development;
        }

        if (environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase) ||
            environmentName.Equals("Prod", StringComparison.OrdinalIgnoreCase))
        {
            return EndpointScope.Production;
        }

        if (environmentName.Equals("Uat", StringComparison.OrdinalIgnoreCase) ||
            environmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase) ||
            environmentName.Equals("Stage", StringComparison.OrdinalIgnoreCase) ||
            environmentName.Equals("Qa", StringComparison.OrdinalIgnoreCase) ||
            environmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
        {
            return EndpointScope.Uat;
        }

        return EndpointScope.Development;
    }
}
