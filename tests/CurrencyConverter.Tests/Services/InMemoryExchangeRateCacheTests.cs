using CurrencyConverter.Tests.TestHelpers;
using TrocaMoedas.Domain.Models;
using TrocaMoedas.Infrastructure.Services;
using Xunit;

namespace CurrencyConverter.Tests.Services;

public class InMemoryExchangeRateCacheTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    private static ExchangeRate Rate(Currency baseCurrency) => new()
    {
        BaseCurrency = baseCurrency,
        Timestamp = Start.UtcDateTime,
        Rates = new Dictionary<Currency, double> { [Currency.USD] = 0.2 }
    };

    [Fact]
    public void TryGet_WhenEmpty_ReturnsFalse()
    {
        var cache = new InMemoryExchangeRateCache(TimeSpan.FromHours(1), new FakeTimeProvider(Start));

        Assert.False(cache.TryGet(Currency.BRL, out var rate));
        Assert.Null(rate);
    }

    [Fact]
    public void TryGet_AfterSet_ReturnsRate()
    {
        var cache = new InMemoryExchangeRateCache(TimeSpan.FromHours(1), new FakeTimeProvider(Start));
        var expected = Rate(Currency.BRL);

        cache.Set(expected);

        Assert.True(cache.TryGet(Currency.BRL, out var cached));
        Assert.Same(expected, cached);
    }

    [Fact]
    public void TryGet_BeforeTtlExpires_ReturnsRate()
    {
        var clock = new FakeTimeProvider(Start);
        var cache = new InMemoryExchangeRateCache(TimeSpan.FromHours(1), clock);
        cache.Set(Rate(Currency.BRL));

        clock.Advance(TimeSpan.FromMinutes(59));

        Assert.True(cache.TryGet(Currency.BRL, out _));
    }

    [Fact]
    public void TryGet_AfterTtlExpires_ReturnsFalseAndRemovesEntry()
    {
        var clock = new FakeTimeProvider(Start);
        var cache = new InMemoryExchangeRateCache(TimeSpan.FromHours(1), clock);
        cache.Set(Rate(Currency.BRL));

        clock.Advance(TimeSpan.FromMinutes(61));

        Assert.False(cache.TryGet(Currency.BRL, out _));

        cache.Set(Rate(Currency.BRL));
        clock.Advance(TimeSpan.FromMinutes(59));
        Assert.True(cache.TryGet(Currency.BRL, out _));
    }

    [Fact]
    public void TryGet_DifferentBaseCurrencies_AreIndependent()
    {
        var cache = new InMemoryExchangeRateCache(TimeSpan.FromHours(1), new FakeTimeProvider(Start));
        cache.Set(Rate(Currency.BRL));

        Assert.False(cache.TryGet(Currency.USD, out _));
    }

    [Fact]
    public void Set_SameBaseCurrency_ReplacesEntry()
    {
        var clock = new FakeTimeProvider(Start);
        var cache = new InMemoryExchangeRateCache(TimeSpan.FromHours(1), clock);
        var first = Rate(Currency.BRL);
        cache.Set(first);

        clock.Advance(TimeSpan.FromMinutes(30));
        var second = Rate(Currency.BRL);
        cache.Set(second);

        Assert.True(cache.TryGet(Currency.BRL, out var cached));
        Assert.Same(second, cached);
    }

    [Fact]
    public void Set_WhenFull_EvictsOldestEntry()
    {
        var clock = new FakeTimeProvider(Start);
        var cache = new InMemoryExchangeRateCache(TimeSpan.FromHours(1), clock, maxEntries: 1);
        cache.Set(Rate(Currency.BRL));

        clock.Advance(TimeSpan.FromMinutes(1));
        cache.Set(Rate(Currency.USD));

        Assert.False(cache.TryGet(Currency.BRL, out _));
        Assert.True(cache.TryGet(Currency.USD, out _));
    }

    [Fact]
    public void Set_WhenFullOfExpiredEntries_StoresNewEntry()
    {
        var clock = new FakeTimeProvider(Start);
        var cache = new InMemoryExchangeRateCache(TimeSpan.FromMinutes(10), clock, maxEntries: 1);
        cache.Set(Rate(Currency.BRL));

        clock.Advance(TimeSpan.FromMinutes(11));
        cache.Set(Rate(Currency.USD));

        Assert.False(cache.TryGet(Currency.BRL, out _));
        Assert.True(cache.TryGet(Currency.USD, out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidTtl_Throws(int minutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InMemoryExchangeRateCache(TimeSpan.FromMinutes(minutes)));
    }

    [Fact]
    public void Constructor_InvalidMaxEntries_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InMemoryExchangeRateCache(TimeSpan.FromHours(1), maxEntries: 0));
    }
}
