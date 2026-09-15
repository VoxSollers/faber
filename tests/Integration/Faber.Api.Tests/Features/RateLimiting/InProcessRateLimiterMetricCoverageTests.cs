using System.Reflection;
using Faber.Modules.Auth.Application.Features.ForgotPassword;
using Faber.Modules.Auth.Application.Features.ForgotPassword.RateLimiting;
using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Notifications.Application.Email;
using Faber.Modules.Notifications.Application.Email.RateLimiting;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Faber.Api.Tests.Features.RateLimiting;

// The third rate-limiting guard, after ResumesRateLimitingCoverageTests (every routed endpoint
// attaches a policy) and RateLimitPolicyRegistrationTests (every declared policy name is registered).
// Those two cover the HTTP middleware path, where RateLimitRejectionHandler records the shared
// rejection counter once for everyone. This one covers what the middleware cannot see: in-process
// limiters that live inside a feature, are called by hand via TryAcquire, and must therefore record
// the metric themselves — the gap issue #397 found in ForgotPasswordEndpoint after the fact.
//
// Why an allowlist instead of inspecting the rejection branch: whether RecordRejection is called on
// the TryAcquire-false path is imperative control flow inside a method body, not route metadata, so
// reflection cannot read it the way ResumesRateLimitingCoverageTests reads EnableRateLimitingAttribute.
// Proving it needs IL inspection (a dependency this repo has never taken) or a behavioural test. We
// take the behavioural test per limiter, and use this guard to make forgetting to write one loud.
//
// Like ResumesRateLimitingCoverageTests, this reads reflection and DI only — it issues no HTTP
// request, so it burns none of the shared budget and claims no client-IP octet range from the
// behavioural suites sharing this fixture.
[Collection<CollectionRateLimiting>]
[Priority(16)]
public class InProcessRateLimiterMetricCoverageTests(WebApp app) : TestBase
{
    /// <summary>
    /// An in-process limiter whose consumer is known to record on the shared rejection counter, with a
    /// behavioural test to prove it. Adding a limiter means adding an entry here AND writing that test.
    /// </summary>
    /// <param name="Contract">The limiter interface discovered by convention.</param>
    /// <param name="Consumer">The type that calls TryAcquire and records the rejection.</param>
    /// <param name="MetricPolicy">Value of the <c>policy</c> tag that consumer records.</param>
    /// <param name="CoveredBy">Fully qualified <c>Namespace.Class.Method</c> of the behavioural test proving the recording; resolved by <see cref="EveryAllowlistedLimiter_ShouldNameAnExistingBehaviouralTest"/> where the assembly is reachable.</param>
    private sealed record InProcessLimiter(Type Contract, Type Consumer, string MetricPolicy, string CoveredBy);

    private static readonly InProcessLimiter[] Allowlist =
    [
        new(
            typeof(ITargetEmailRateLimiter),
            typeof(ForgotPasswordEndpoint),
            ForgotPasswordEndpoint.MetricPolicy,
            "Faber.Api.Tests.Features.RateLimiting.ForgotPasswordRateLimitingTests"
            + ".EmailBombing_PerEmailThrottle_ShouldRecordRejectionMetric"),
        new(
            typeof(IRecipientEmailRateLimiter),
            typeof(ThrottledEmailSender),
            ThrottledEmailSender.MetricPolicy,
            "Faber.Modules.Notifications.Application.UnitTests.Email.ThrottledEmailSenderTests"
            + ".ThrottledSend_ShouldRecordRejectionMetric")
    ];

    private static readonly HashSet<string> HttpPolicyNames = typeof(RateLimitPolicies)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToHashSet(StringComparer.Ordinal);

    [Fact]
    [Priority(1)]
    public void EveryInProcessLimiterContract_ShouldBeOnTheMetricCoverageAllowlist()
    {
        var discovered = DiscoveredContracts().ToHashSet();
        var allowlisted = Allowlist.Select(entry => entry.Contract).ToHashSet();

        var undeclared = discovered
            .Except(allowlisted)
            .Select(type => type.FullName!)
            .Order(StringComparer.Ordinal)
            .ToList();

        undeclared.ShouldBeEmpty(
            "These in-process rate limiters are not on this test's Allowlist. An in-process limiter "
            + "never passes through RequireRateLimiting, so RateLimitRejectionHandler cannot record its "
            + "rejection — the consumer must call RateLimitingMetrics.RecordRejection itself on the "
            + "TryAcquire-false branch with its own distinct policy tag (issue #416). Write a "
            + "MeterListener test proving that, then add an Allowlist entry pointing at it:"
            + $"{Environment.NewLine}{string.Join(Environment.NewLine, undeclared)}");

        // The other direction: a stale entry means the limiter was deleted or renamed, and the guard
        // would otherwise keep passing while silently covering nothing.
        var stale = allowlisted
            .Except(discovered)
            .Select(type => type.FullName!)
            .Order(StringComparer.Ordinal)
            .ToList();

        stale.ShouldBeEmpty(
            "These Allowlist entries no longer match a discoverable in-process rate limiter. Remove "
            + "the entry if the limiter is gone, or fix the discovery convention if it was renamed:"
            + $"{Environment.NewLine}{string.Join(Environment.NewLine, stale)}");
    }

    [Fact]
    [Priority(2)]
    public void EveryInProcessLimiterImplementation_ShouldExposeADiscoverableContract()
    {
        // Discovery keys on interfaces, so a concrete limiter written without one would be invisible
        // to the assertion above. This keeps the convention honest.
        var contracts = DiscoveredContracts().ToHashSet();

        var contractless = ModuleAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.Name.EndsWith("RateLimiter", StringComparison.Ordinal))
            .Where(type => !type.GetInterfaces().Any(contracts.Contains))
            .Select(type => type.FullName!)
            .Order(StringComparer.Ordinal)
            .ToList();

        contractless.ShouldBeEmpty(
            "These rate limiter classes implement no I*RateLimiter interface, so the coverage guard "
            + "cannot see them. Give each one an interface named I{Name}RateLimiter in a RateLimiting "
            + $"namespace, then add it to the Allowlist:{Environment.NewLine}"
            + string.Join(Environment.NewLine, contractless));
    }

    [Fact]
    [Priority(3)]
    public void EveryAllowlistedLimiter_ShouldRecordADistinctMetricPolicyTag()
    {
        Allowlist.ShouldNotBeEmpty();

        foreach (var entry in Allowlist)
        {
            entry.MetricPolicy.ShouldNotBeNullOrWhiteSpace(
                $"{entry.Consumer.FullName} declares an empty metric policy tag.");
            entry.CoveredBy.ShouldNotBeNullOrWhiteSpace(
                $"{entry.Consumer.FullName} names no behavioural test proving it records.");
        }

        // A tag reused across limiters, or colliding with an HTTP policy name, collapses two layers
        // into one dashboard series and destroys the attribution the tag exists for.
        var duplicates = Allowlist
            .GroupBy(entry => entry.MetricPolicy, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order(StringComparer.Ordinal)
            .ToList();

        duplicates.ShouldBeEmpty(
            "These metric policy tags are shared by more than one in-process limiter, so a rejection "
            + $"cannot be attributed to the layer that fired:{Environment.NewLine}"
            + string.Join(Environment.NewLine, duplicates));

        var collisions = Allowlist
            .Where(entry => HttpPolicyNames.Contains(entry.MetricPolicy))
            .Select(entry => $"{entry.Consumer.FullName} -> '{entry.MetricPolicy}'")
            .Order(StringComparer.Ordinal)
            .ToList();

        collisions.ShouldBeEmpty(
            "These in-process limiters record a tag that is also a RateLimitPolicies constant, so their "
            + "rejections are indistinguishable from the HTTP middleware's on the same policy:"
            + $"{Environment.NewLine}{string.Join(Environment.NewLine, collisions)}");
    }

    [Fact]
    [Priority(4)]
    public void EveryAllowlistedLimiterContract_ShouldBeRegisteredInTheHost()
    {
        // An allowlisted contract that nothing registers can never reject anything, which would make
        // its behavioural test a museum piece. Resolving from the live host proves the limiter is wired.
        using var scope = app.Services.CreateScope();

        foreach (var entry in Allowlist)
        {
            scope.ServiceProvider.GetService(entry.Contract).ShouldNotBeNull(
                $"{entry.Contract.FullName} is on the coverage allowlist but is not registered in DI, "
                + "so it never runs and its metric test proves nothing about production.");
        }
    }

    [Fact]
    [Priority(5)]
    public void EveryAllowlistedLimiter_ShouldNameAnExistingBehaviouralTest()
    {
        // The Allowlist's CoveredBy string is the only link between "this limiter is known" and "and
        // here is the proof it records". Left unresolved it is a comment: an entry naming a test that
        // was renamed, moved, or never written would pass, which is the #397 failure mode moved one
        // level up. Resolving it turns the claim into something the build can refuse.
        var verified = 0;
        var missing = new List<string>();

        foreach (var entry in Allowlist)
        {
            var separator = entry.CoveredBy.LastIndexOf('.');
            separator.ShouldBeGreaterThan(0,
                $"'{entry.CoveredBy}' is not a Namespace.Class.Method reference.");

            var typeName = entry.CoveredBy[..separator];
            var methodName = entry.CoveredBy[(separator + 1)..];
            var declaringType = ResolveLoadedType(typeName);

            // Not every behavioural test is reachable from here: ThrottledEmailSender's lives in the
            // Notifications unit-test assembly, which this integration project deliberately does not
            // reference. An unreachable entry is unverifiable rather than wrong — the count assertion
            // below is what stops "unverifiable" from quietly becoming "unchecked".
            if (declaringType is null)
            {
                continue;
            }

            var method = declaringType.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            if (method is null)
            {
                missing.Add($"{entry.Contract.FullName} -> '{entry.CoveredBy}'");

                continue;
            }

            verified++;
        }

        missing.ShouldBeEmpty(
            "These Allowlist entries name a behavioural test that does not exist on the resolved type. "
            + "Point CoveredBy at the real MeterListener test proving the limiter records its "
            + $"rejection, or write that test:{Environment.NewLine}{string.Join(Environment.NewLine, missing)}");

        verified.ShouldBeGreaterThan(0,
            "No Allowlist entry's CoveredBy test could be resolved, so this fact proved nothing. Either "
            + "the type names drifted, or every entry now lives in an assembly this project cannot see — "
            + "in which case move the guard to a project that can see at least one of them.");
    }

    private static IEnumerable<Type> DiscoveredContracts()
    {
        return ModuleAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type => type.IsInterface
                           && type.Name.EndsWith("RateLimiter", StringComparison.Ordinal));
    }

    // Two independent sources, unioned, because either one alone can miss a module.
    //
    // The reference walk (Faber.Api outward) is pruned by the C# compiler: an assembly a project
    // references but never uses a type from gets no AssemblyRef row, so GetReferencedAssemblies()
    // silently omits it — a limiter interface parked in an otherwise-unused layer would vanish from
    // discovery and this guard would pass while covering nothing. The directory scan closes that hole:
    // MSBuild copies transitive project outputs next to the test assembly whether or not the compiler
    // kept the reference. The walk is kept because it still resolves an assembly that was loaded but,
    // for any build-layout reason, was not copied.
    //
    // Neither source reaches a module the API host does not ship, which is deliberate: a limiter the
    // API never loads cannot reject an API request.
    private static IReadOnlyList<Assembly> ModuleAssemblies()
    {
        var byName = new Dictionary<string, Assembly>(StringComparer.Ordinal);

        foreach (var assembly in ReferencedAssemblies().Concat(DeployedModuleAssemblies()))
        {
            var name = assembly.GetName().Name;

            if (name?.StartsWith("Faber.Modules.", StringComparison.Ordinal) != true)
            {
                continue;
            }

            byName.TryAdd(name, assembly);
        }

        return byName.Values.ToList();
    }

    private static IEnumerable<Assembly> ReferencedAssemblies()
    {
        var seen = new Dictionary<string, Assembly>(StringComparer.Ordinal);
        var pending = new Queue<Assembly>();
        pending.Enqueue(typeof(Faber.Api.RateLimiting.RateLimitRejectionHandler).Assembly);

        while (pending.Count > 0)
        {
            foreach (var reference in pending.Dequeue().GetReferencedAssemblies())
            {
                if (reference.Name?.StartsWith("Faber.", StringComparison.Ordinal) != true
                    || seen.ContainsKey(reference.FullName))
                {
                    continue;
                }

                var loaded = Assembly.Load(reference);
                seen[reference.FullName] = loaded;
                pending.Enqueue(loaded);
            }
        }

        return seen.Values.ToList();
    }

    private static IEnumerable<Assembly> DeployedModuleAssemblies()
    {
        var loaded = new List<Assembly>();

        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "Faber.Modules.*.dll"))
        {
            // BadImageFormatException means the file is not a managed Faber assembly at all (a native or
            // resource-only artefact that matched the glob), so skipping loses nothing. FileLoadException is
            // broader: a genuine module DLL that is stale or duplicated under one simple name lands here too.
            // Skipping is still right — the reference walk above resolves the copy the host actually loaded,
            // and a build broken enough to produce that state fails long before this guard runs.
            //
            // LoadFrom, never LoadFile: LoadFrom returns the instance already in the default load context for
            // an assembly of the same identity, so the Type objects it yields stay reference-equal to the
            // typeof(...) entries in Allowlist. LoadFile would mint a second, distinct Type graph and every
            // set comparison in this class would silently start missing.
            try
            {
                loaded.Add(Assembly.LoadFrom(path));
            }
            catch (BadImageFormatException)
            {
                continue;
            }
            catch (FileLoadException)
            {
                continue;
            }
        }

        return loaded;
    }

    // Scans loaded assemblies rather than using Type.GetType, which only searches the calling assembly
    // and the core library unless the name is assembly-qualified — and the Allowlist deliberately
    // stores plain, readable names.
    private static Type? ResolveLoadedType(string fullName)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(fullName, throwOnError: false))
            .FirstOrDefault(type => type is not null);
    }

    // A partially loadable assembly yields the types that did load rather than taking down the whole
    // scan. The trade-off is that a limiter interface whose containing type graph failed to load would
    // go unseen — acceptable because every assembly scanned here is a same-repo project built alongside
    // this test, where a load failure would already be breaking the host itself.
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }
}
