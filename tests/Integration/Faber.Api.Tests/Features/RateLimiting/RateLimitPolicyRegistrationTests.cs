using System.Collections;
using System.Reflection;
using Faber.Modules.Common.PublicApi.RateLimiting;
using FastEndpoints.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Faber.Api.Tests.Features.RateLimiting;

// The mirror image of ResumesRateLimitingCoverageTests: that suite proves every routed endpoint's
// policy name is a declared RateLimitPolicies constant; this one proves every declared constant is
// actually registered with a policy class by AddFaberRateLimiting. Without it, adding a constant
// without the matching limiter.AddPolicy<string, TPolicy>(...) call compiles clean, an endpoint can
// opt into it via RequireRateLimiting, and the app only discovers the gap at request time in
// production instead of in CI.
[Collection<CollectionRateLimiting>]
[Priority(15)]
public class RateLimitPolicyRegistrationTests(WebApp app) : TestBase
{
    private static readonly HashSet<string> DeclaredPolicyNames = typeof(RateLimitPolicies)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToHashSet(StringComparer.Ordinal);

    [Fact]
    [Priority(1)]
    public void EveryRateLimitPolicyConstant_ShouldBeRegisteredByAddFaberRateLimiting()
    {
        DeclaredPolicyNames.ShouldNotBeEmpty();

        var unregistered = DeclaredPolicyNames
            .Except(RegisteredPolicyNames(), StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        unregistered.ShouldBeEmpty(
            "These RateLimitPolicies constants have no matching limiter.AddPolicy<string, TPolicy>(...) call "
            + "in AddFaberRateLimiting, so an endpoint could opt in via RequireRateLimiting to a name the "
            + $"limiter has never heard of and fail only at request time:{Environment.NewLine}"
            + string.Join(Environment.NewLine, unregistered));
    }

    // RateLimiterOptions exposes no public way to enumerate registered policy names: AddPolicy<string, TPolicy>
    // stores a factory in the internal UnactivatedPolicyMap, which migrates into the also-internal PolicyMap
    // only once a request actually resolves that policy. Reading both via reflection is the only way to see
    // the full registered set without issuing a request per policy, which the shared budget fixture forbids.
    private HashSet<string> RegisteredPolicyNames()
    {
        var options = app.Services.GetRequiredService<IOptions<RateLimiterOptions>>().Value;
        var optionsType = typeof(RateLimiterOptions);

        var policyMap = (IDictionary)optionsType
            .GetProperty("PolicyMap", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(options)!;

        var unactivatedPolicyMap = (IDictionary)optionsType
            .GetProperty("UnactivatedPolicyMap", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(options)!;

        return policyMap.Keys.Cast<string>()
            .Concat(unactivatedPolicyMap.Keys.Cast<string>())
            .ToHashSet(StringComparer.Ordinal);
    }
}
