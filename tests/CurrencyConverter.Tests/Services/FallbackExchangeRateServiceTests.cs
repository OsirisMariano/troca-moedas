using TrocaMoedas.Domain.Models;
using TrocaMoedas.Infrastructure.Services;
using Xunit;

namespace CurrencyConverter.Tests.Services;

public class FallbackExchangeRateServiceTests
{
    private const double Tolerance = 6;

    private readonly FallbackExchangeRateService _service = new();

    [Fact]
    public async Task GetRatesAsync_WithBrlBase_ReturnsInverseRatesPerCurrency()
    {
        var rate = await _service.GetRatesAsync(Currency.BRL);

        Assert.Equal(Currency.BRL, rate.BaseCurrency);
        Assert.Equal(0.2, rate.GetRate(Currency.BRL, Currency.USD), Tolerance);
        Assert.Equal(1.0 / 5.4, rate.GetRate(Currency.BRL, Currency.EUR), Tolerance);
        Assert.Equal(1.0 / 6.3, rate.GetRate(Currency.BRL, Currency.GBP), Tolerance);
        Assert.Equal(1.0 / 0.034, rate.GetRate(Currency.BRL, Currency.JPY), Tolerance);
        Assert.Equal(1.0 / 0.005, rate.GetRate(Currency.BRL, Currency.ARS), Tolerance);
    }

    [Fact]
    public async Task GetRatesAsync_WithNonBrlBase_ReturnsBrlRateRelativeToBase()
    {
        var rate = await _service.GetRatesAsync(Currency.USD);

        Assert.Equal(Currency.USD, rate.BaseCurrency);
        Assert.Equal(5.0, rate.GetRate(Currency.USD, Currency.BRL), Tolerance);
        Assert.Equal(5.0 / 5.4, rate.GetRate(Currency.USD, Currency.EUR), Tolerance);
        Assert.Equal(5.0 / 0.034, rate.GetRate(Currency.USD, Currency.JPY), Tolerance);
    }

    [Fact]
    public void Convert_SameCurrency_ReturnsAmount()
    {
        var result = _service.Convert(Currency.EUR, Currency.EUR, 100);

        Assert.Equal(100, result, Tolerance);
    }

    [Fact]
    public void Convert_BrlToUsd_UsesInverseFallbackRate()
    {
        var result = _service.Convert(Currency.BRL, Currency.USD, 100);

        Assert.Equal(20, result, Tolerance);
    }

    [Fact]
    public void Convert_UsdToBrl_UsesFallbackRate()
    {
        var result = _service.Convert(Currency.USD, Currency.BRL, 100);

        Assert.Equal(500, result, Tolerance);
    }

    [Fact]
    public void Convert_CrossCurrency_ConvertsThroughBrl()
    {
        var result = _service.Convert(Currency.USD, Currency.EUR, 100);

        Assert.Equal(500.0 / 5.4, result, Tolerance);
    }

    [Fact]
    public async Task Convert_And_GetRate_ProduceConsistentResults()
    {
        const double amount = 250;

        var direct = _service.Convert(Currency.BRL, Currency.USD, amount);

        var exchangeRate = await _service.GetRatesAsync(Currency.BRL);
        var viaRate = amount * exchangeRate.GetRate(Currency.BRL, Currency.USD);

        Assert.Equal(direct, viaRate, Tolerance);
    }

    [Fact]
    public async Task GetRatesAsync_DoesNotIncludeRatesForBaseCurrency_WhenBaseIsInFallback()
    {
        var rate = await _service.GetRatesAsync(Currency.USD);

        Assert.False(rate.Rates.ContainsKey(Currency.USD));
    }

    [Fact]
    public async Task GetRatesAsync_SetsTimestampToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var rate = await _service.GetRatesAsync(Currency.BRL);

        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(rate.Timestamp, before, after);
    }
}