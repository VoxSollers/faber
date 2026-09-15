using System.Reflection;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Faber.Api.Tests.Features.RateLimiting;

// A structural guard rather than a behavioural one: it reads the built route table instead of
// issuing requests, so it burns no rate-limit budget and claims no client-IP octet range from the
// behavioural suites sharing this fixture.
//
// The check has to run against the *built* route table. `Options(x => x.RequireRateLimiting(...))`
// is a RouteHandlerBuilder callback that FastEndpoints defers until route-mapping time, so neither
// the endpoint source text nor a pre-build EndpointDefinition can prove a policy was attached.
[Collection<CollectionRateLimiting>]
[Priority(14)]
public class ResumesRateLimitingCoverageTests(WebApp app) : TestBase
{
    private static readonly Assembly ResumesAssembly = typeof(ResumesGroup).Assembly;

    private static readonly HashSet<string> KnownPolicyNames = typeof(RateLimitPolicies)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToHashSet(StringComparer.Ordinal);

    [Fact]
    [Priority(1)]
    public void EveryResumesEndpointType_ShouldAppearInTheRouteTable()
    {
        // Without this, a broken assembly filter would make the two policy assertions below pass
        // vacuously over an empty set — the classic silent death of a reflection-based guard.
        var declared = ResumesAssembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false } && typeof(IEndpoint).IsAssignableFrom(type))
            .ToHashSet();

        declared.ShouldNotBeEmpty();

        var routed = ResumesRoutes().Select(route => route.EndpointType).ToHashSet();

        var unrouted = declared
            .Except(routed)
            .Select(type => type.FullName!)
            .Order(StringComparer.Ordinal)
            .ToList();

        unrouted.ShouldBeEmpty(
            "These Resumes endpoint classes never reached the route table, so no rate-limiting "
            + $"assertion can cover them:{Environment.NewLine}{string.Join(Environment.NewLine, unrouted)}");
    }

    [Fact]
    [Priority(2)]
    public void EveryResumesEndpoint_ShouldAttachARateLimitingPolicy()
    {
        var unprotected = ResumesRoutes()
            .Where(route => route.RateLimiting is null
                            || string.IsNullOrWhiteSpace(route.RateLimiting.PolicyName)
                            || route.RateLimitingDisabled is not null)
            .Select(route => $"{route.EndpointType.FullName} ({route.DisplayName})")
            .Order(StringComparer.Ordinal)
            .ToList();

        unprotected.ShouldBeEmpty(
            "These Resumes endpoints are routed without an effective rate-limiting policy. Add "
            + "Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthenticatedDefault)) to "
            + $"Configure() — or RateLimitPolicies.ExpensiveResource for costly work:{Environment.NewLine}"
            + string.Join(Environment.NewLine, unprotected));
    }

    [Fact]
    [Priority(3)]
    public void EveryResumesEndpoint_ShouldUseAKnownRateLimitPolicyName()
    {
        // A typo'd policy name is worse than a missing one: RequireRateLimiting happily attaches it
        // and the request then falls through to the global baseline with no per-endpoint ceiling.
        var unknown = ResumesRoutes()
            .Where(route => route.RateLimiting?.PolicyName is { Length: > 0 } name
                            && !KnownPolicyNames.Contains(name))
            .Select(route => $"{route.EndpointType.FullName} -> '{route.RateLimiting!.PolicyName}'")
            .Order(StringComparer.Ordinal)
            .ToList();

        unknown.ShouldBeEmpty(
            "These Resumes endpoints reference a policy name that Faber.Api never registers. Use a "
            + $"RateLimitPolicies constant:{Environment.NewLine}{string.Join(Environment.NewLine, unknown)}");
    }

    private IReadOnlyList<RoutedResumesEndpoint> ResumesRoutes()
    {
        return app.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .Select(endpoint => (Endpoint: endpoint, Definition: endpoint.Metadata.GetMetadata<EndpointDefinition>()))
            .Where(candidate => candidate.Definition?.EndpointType.Assembly == ResumesAssembly)
            .Select(candidate => new RoutedResumesEndpoint(
                candidate.Definition!.EndpointType,
                candidate.Endpoint.DisplayName ?? candidate.Definition.EndpointType.Name,
                candidate.Endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>(),
                candidate.Endpoint.Metadata.GetMetadata<DisableRateLimitingAttribute>()))
            .ToList();
    }

    private sealed record RoutedResumesEndpoint(
        Type EndpointType,
        string DisplayName,
        EnableRateLimitingAttribute? RateLimiting,
        DisableRateLimitingAttribute? RateLimitingDisabled);
}
