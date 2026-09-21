using Xunit;
using TrocaMoedas.Domain.Models;

namespace CurrencyConverter.Tests.Domain;

public class ExchangeRateTests
{
    [Theory]
    [InlineData(Currency.BRL, Currency.USD)]
    [InlineData(Currency.USD, Currency.EUR)]
    [InlineData(Currency.JPY, Currency.ARS)]
    public void GetRate_WhenFromEqualsTo_ReturnsOne(Currency baseCurrency, Currency currency)
    {
        var rate = CreateRate(baseCurrency);

        var result = rate.GetRate(currency, currency);

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void GetRate_WhenFromIsBaseCurrency_ReturnsDirectRate()
    {
        var rate = CreateRate(Currency.BRL);

        var result = rate.GetRate(Currency.BRL, Currency.USD);

        Assert.Equal(5.0, result);
    }

    [Fact]
    public void GetRate_WhenToIsBaseCurrency_ReturnsInverseRate()
    {
        var rate = CreateRate(Currency.BRL);

        var result = rate.GetRate(Currency.USD, Currency.BRL);

        Assert.Equal(0.2, result, precision: 6);
    }

    [Fact]
    public void GetRate_WhenNeitherIsBaseCurrency_ReturnsCrossRate()
    {
        var rate = CreateRate(Currency.BRL);

        var result = rate.GetRate(Currency.USD, Currency.EUR);

        Assert.Equal(1.08, result, precision: 6);
    }

    [Fact]
    public void GetRate_WhenRateIsMissing_ThrowsInvalidOperationException()
    {
        var rate = new ExchangeRate
        {
            BaseCurrency = Currency.BRL,
            Rates = new Dictionary<Currency, double>
            {
                [Currency.USD] = 5.0
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => rate.GetRate(Currency.BRL, Currency.ARS));

        Assert.Contains("ARS", exception.Message);
    }

    [Fact]
    public void GetRate_WhenBaseCurrencyMissingFromRates_UsesInverseForTarget()
    {
        var rate = new ExchangeRate
        {
            BaseCurrency = Currency.BRL,
            Rates = new Dictionary<Currency, double>
            {
                [Currency.ARS] = 0.005
            }
        };

        var result = rate.GetRate(Currency.BRL, Currency.ARS);

        Assert.Equal(0.005, result, precision: 6);
    }

    [Fact]
    public void Timestamp_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var rate = new ExchangeRate
        {
            BaseCurrency = Currency.BRL,
            Rates = new Dictionary<Currency, double>()
        };

        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(rate.Timestamp, before, after);
    }

    private static ExchangeRate CreateRate(Currency baseCurrency)
    {
        return new ExchangeRate
        {
            BaseCurrency = baseCurrency,
            Rates = new Dictionary<Currency, double>
            {
                [Currency.USD] = 5.0,
                [Currency.EUR] = 5.4,
                [Currency.GBP] = 6.3,
                [Currency.JPY] = 0.034,
                [Currency.ARS] = 0.005
            }
        };
    }
}