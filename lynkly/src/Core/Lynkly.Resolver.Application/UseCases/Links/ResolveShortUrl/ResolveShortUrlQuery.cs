using Lynkly.Shared.Kernel.MediatR.Abstractions;

namespace Lynkly.Resolver.Application.UseCases.Links.ResolveShortUrl;

public sealed record ResolveShortUrlQuery(
    string Alias,
    int? CacheDurationSeconds) : IRequest<ResolveShortUrlResult?>;
