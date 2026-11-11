using System.Text.Json;
using Moq;
using TrocaMoedas.Application.Services;
using TrocaMoedas.Domain.Models;
using TrocaMoedas.Infrastructure.Services;
using Xunit;

namespace CurrencyConverter.Tests.Services;

public class ResilientExchangeRateServiceTests
{
    private static ExchangeRate Rate(Currency baseCurrency) => new()
    {
        BaseCurrency = baseCurrency,
        Rates = new Dictionary<Currency, double> { [Currency.USD] = 0.2 }
    };

    [Fact]
    public async Task GetRatesAsync_WhenPrimarySucceeds_DoesNotUseFallback()
    {
        var expected = Rate(Currency.BRL);

        var primary = new Mock<IExchangeRateService>();
        primary.Setup(service => service.GetRatesAsync(Currency.BRL)).ReturnsAsync(expected);

        var fallback = new Mock<IExchangeRateService>(MockBehavior.Strict);

        var service = new ResilientExchangeRateService(primary.Object, fallback.Object);

        var result = await service.GetRatesAsync(Currency.BRL);

        Assert.Same(expected, result);
        fallback.Verify(service => service.GetRatesAsync(It.IsAny<Currency>()), Times.Never);
    }

    [Fact]
    public async Task GetRatesAsync_WhenPrimaryFailsWithRecoverableError_UsesFallback()
    {
        var fallbackRate = Rate(Currency.BRL);
        var exceptions = new Exception[]
        {
            new HttpRequestException("403 (Forbidden)"),
            new TaskCanceledException("timeout"),
            new IOException("network"),
            new JsonException("invalid json"),
            new KeyNotFoundException("conversion_rates")
        };

        foreach (var exception in exceptions)
        {
            var primary = new Mock<IExchangeRateService>();
            primary.Setup(service => service.GetRatesAsync(Currency.BRL)).ThrowsAsync(exception);

            var fallback = new Mock<IExchangeRateService>();
            fallback.Setup(service => service.GetRatesAsync(Currency.BRL)).ReturnsAsync(fallbackRate);

            var service = new ResilientExchangeRateService(primary.Object, fallback.Object);

            var result = await service.GetRatesAsync(Currency.BRL);

            Assert.Same(fallbackRate, result);
            fallback.Verify(service => service.GetRatesAsync(Currency.BRL), Times.Once);
        }
    }

    [Fact]
    public async Task GetRatesAsync_WhenPrimaryFailsWithUnexpectedError_Propagates()
    {
        var primary = new Mock<IExchangeRateService>();
        primary.Setup(service => service.GetRatesAsync(Currency.BRL))
            .ThrowsAsync(new InvalidOperationException("bug"));

        var fallback = new Mock<IExchangeRateService>(MockBehavior.Strict);

        var service = new ResilientExchangeRateService(primary.Object, fallback.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetRatesAsync(Currency.BRL));
    }

    [Fact]
    public async Task GetRatesAsync_WhenFallingBack_LogsMessage()
    {
        var logs = new List<string>();

        var primary = new Mock<IExchangeRateService>();
        primary.Setup(service => service.GetRatesAsync(Currency.BRL))
            .ThrowsAsync(new HttpRequestException("503"));

        var fallback = new Mock<IExchangeRateService>();
        fallback.Setup(service => service.GetRatesAsync(Currency.BRL)).ReturnsAsync(Rate(Currency.BRL));

        var service = new ResilientExchangeRateService(primary.Object, fallback.Object, logs.Add);

        await service.GetRatesAsync(Currency.BRL);

        Assert.Single(logs);
        Assert.Contains("fallback", logs[0]);
    }

    [Fact]
    public void Convert_DelegatesToFallback()
    {
        var primary = new Mock<IExchangeRateService>(MockBehavior.Strict);
        var fallback = new Mock<IExchangeRateService>();
        fallback.Setup(service => service.Convert(Currency.BRL, Currency.USD, 100)).Returns(20);

        var service = new ResilientExchangeRateService(primary.Object, fallback.Object);

        Assert.Equal(20, service.Convert(Currency.BRL, Currency.USD, 100));
    }
}
