using System.Net;
using System.Text;
using System.Text.Json;
using TrocaMoedas.Domain.Models;
using TrocaMoedas.Infrastructure.Services;
using Xunit;

namespace CurrencyConverter.Tests.Services;

public class ExchangeRateApiServiceTests
{
    private const string ApiKey = "test-key";

    [Fact]
    public async Task GetRatesAsync_OnSuccess_ParsesRatesExcludingBaseCurrency()
    {
        var payload = JsonDocument.Parse("""
            {
              "result": "success",
              "base_code": "BRL",
              "conversion_rates": {
                "USD": 5.0,
                "EUR": 5.4,
                "GBP": 6.3,
                "JPY": 0.034,
                "ARS": 0.005
              }
            }
            """).RootElement;

        using var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload.GetRawText(), Encoding.UTF8, "application/json")
        });

        var service = new ExchangeRateApiService(new HttpClient(handler), ApiKey);

        var rate = await service.GetRatesAsync(Currency.BRL);

        Assert.Equal(Currency.BRL, rate.BaseCurrency);
        Assert.Equal(5.0, rate.Rates[Currency.USD], precision: 6);
        Assert.Equal(5.4, rate.Rates[Currency.EUR], precision: 6);
        Assert.Equal(6.3, rate.Rates[Currency.GBP], precision: 6);
        Assert.Equal(0.034, rate.Rates[Currency.JPY], precision: 6);
        Assert.Equal(0.005, rate.Rates[Currency.ARS], precision: 6);
        Assert.False(rate.Rates.ContainsKey(Currency.BRL));
    }

    [Fact]
    public async Task GetRatesAsync_BuildsUrlWithApiKeyAndBaseCode()
    {
        string? requestedUrl = null;

        using var handler = new FakeHttpMessageHandler(request =>
        {
            requestedUrl = request.RequestUri?.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "conversion_rates": { "USD": 1.0 } }""", Encoding.UTF8, "application/json")
            };
        });

        var service = new ExchangeRateApiService(new HttpClient(handler), ApiKey);

        await service.GetRatesAsync(Currency.EUR);

        Assert.Contains($"/v6/{ApiKey}/latest/EUR", requestedUrl);
    }

    [Fact]
    public async Task GetRatesAsync_OnServerError_Throws()
    {
        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = new ExchangeRateApiService(new HttpClient(handler), ApiKey);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetRatesAsync(Currency.BRL));
    }

    [Fact]
    public async Task GetRatesAsync_OnMissingConversionRates_Throws()
    {
        using var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "result": "error" }""", Encoding.UTF8, "application/json")
        });

        var service = new ExchangeRateApiService(new HttpClient(handler), ApiKey);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetRatesAsync(Currency.BRL));
    }

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(responder(request));
        }
    }
}