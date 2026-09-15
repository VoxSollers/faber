using Faber.Modules.Common.PublicApi;
using Faber.Modules.Notifications.Application.Email.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Faber.Modules.Notifications.Application.UnitTests.Email.RateLimiting;

public class RecipientEmailRateLimiterTests
{
    private const string Recipient = "victim-inbox@example.com";

    private static RecipientEmailRateLimiter CreateLimiter(int tokenLimit = 3, bool enabled = true)
    {
        return new RecipientEmailRateLimiter(Options.Create(new RecipientEmailRateLimitingOptions
        {
            Enabled = enabled,
            TokenLimit = tokenLimit,
            TokensPerPeriod = tokenLimit,
            ReplenishmentPeriodSeconds = 3600
        }));
    }

    private static RecipientEmailRateLimitingOptions Bind(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var options = new RecipientEmailRateLimitingOptions();
        new RecipientEmailRateLimitingOptionsSetup(configuration).Configure(options);

        return options;
    }

    [Fact]
    public void WithinTokenLimit_ShouldAcquireEveryTime()
    {
        using var limiter = CreateLimiter(tokenLimit: 3);

        for (var i = 0; i < 3; i++)
        {
            limiter.TryAcquire(Recipient).ShouldBeTrue();
        }
    }

    [Fact]
    public void ExceedingTokenLimit_ShouldRejectFurtherSends()
    {
        using var limiter = CreateLimiter(tokenLimit: 3);

        for (var i = 0; i < 3; i++)
        {
            limiter.TryAcquire(Recipient).ShouldBeTrue();
        }

        limiter.TryAcquire(Recipient).ShouldBeFalse();
    }

    [Fact]
    public void ExhaustedRecipient_ShouldNotAffectAnotherRecipient()
    {
        using var limiter = CreateLimiter(tokenLimit: 2);

        limiter.TryAcquire(Recipient).ShouldBeTrue();
        limiter.TryAcquire(Recipient).ShouldBeTrue();
        limiter.TryAcquire(Recipient).ShouldBeFalse();

        limiter.TryAcquire("someone-else@example.com").ShouldBeTrue();
    }

    [Theory]
    [InlineData("Victim-Inbox@Example.com")]
    [InlineData(" VICTIM-INBOX@EXAMPLE.COM ")]
    [InlineData("victim-inbox@example.com ")]
    public void CaseAndWhitespaceVariants_ShouldShareTheSameBucket(string variant)
    {
        // An attacker must not be able to mint a fresh budget by varying casing or padding
        // the address — every variant has to normalize onto the same partition key.
        using var limiter = CreateLimiter(tokenLimit: 2);

        limiter.TryAcquire(Recipient).ShouldBeTrue();
        limiter.TryAcquire(Recipient).ShouldBeTrue();

        limiter.TryAcquire(variant).ShouldBeFalse();
    }

    [Fact]
    public void DisabledLimiter_ShouldAlwaysAcquire()
    {
        // Mirrors the #330 test-environment relaxation: with the master switch off the
        // limiter must be a no-op, never a bucket that silently drains.
        using var limiter = CreateLimiter(tokenLimit: 1, enabled: false);

        for (var i = 0; i < 50; i++)
        {
            limiter.TryAcquire(Recipient).ShouldBeTrue();
        }
    }

    [Fact]
    public void RetryAfterSeconds_ShouldMatchReplenishmentPeriod()
    {
        using var limiter = CreateLimiter();

        limiter.RetryAfterSeconds.ShouldBe(3600);
    }

    [Fact]
    public void OptionsSetup_ShouldBindOutboundEmailSection()
    {
        var options = Bind(new Dictionary<string, string?>
        {
            [$"{ConfigurationConstants.Sections.RateLimiting}:Enabled"] = "true",
            [$"{ConfigurationConstants.Sections.RateLimiting}:OutboundEmail:TokenLimit"] = "7",
            [$"{ConfigurationConstants.Sections.RateLimiting}:OutboundEmail:TokensPerPeriod"] = "7",
            [$"{ConfigurationConstants.Sections.RateLimiting}:OutboundEmail:ReplenishmentPeriodSeconds"] = "1800"
        });

        options.Enabled.ShouldBeTrue();
        options.TokenLimit.ShouldBe(7);
        options.TokensPerPeriod.ShouldBe(7);
        options.ReplenishmentPeriodSeconds.ShouldBe(1800);
    }

    [Fact]
    public void OptionsSetup_ShouldInheritDisabledFromSharedRateLimitingSwitch()
    {
        var options = Bind(new Dictionary<string, string?>
        {
            [$"{ConfigurationConstants.Sections.RateLimiting}:Enabled"] = "false"
        });

        options.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void OptionsSetup_ShouldAllowIndependentOverrideOfEnabled()
    {
        var options = Bind(new Dictionary<string, string?>
        {
            [$"{ConfigurationConstants.Sections.RateLimiting}:Enabled"] = "false",
            [$"{ConfigurationConstants.Sections.RateLimiting}:OutboundEmail:Enabled"] = "true"
        });

        options.Enabled.ShouldBeTrue();
    }
}
