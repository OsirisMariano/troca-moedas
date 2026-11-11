using System.Text.Json;
using TrocaMoedas.Application.Services;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Infrastructure.Services;

public class ExchangeRateApiService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IExchangeRateCache? _cache;

    private static readonly Dictionary<Currency, string> CurrencyCodes = new()
    {
        [Currency.BRL] = "BRL",
        [Currency.USD] = "USD",
        [Currency.EUR] = "EUR",
        [Currency.GBP] = "GBP",
        [Currency.JPY] = "JPY",
        [Currency.ARS] = "ARS"
    };

    public ExchangeRateApiService(HttpClient httpClient, string apiKey, IExchangeRateCache? cache = null)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _cache = cache;
    }

    public async Task<ExchangeRate> GetRatesAsync(Currency baseCurrency)
    {
        if (_cache is not null && _cache.TryGet(baseCurrency, out var cached))
        {
            return cached;
        }

        var code = CurrencyCodes[baseCurrency];
        var url = $"https://v6.exchangerate-api.com/v6/{_apiKey}/latest/{code}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        var rates = root.GetProperty("conversion_rates");

        var exchangeRate = new ExchangeRate
        {
            BaseCurrency = baseCurrency,
            Timestamp = DateTime.UtcNow
        };

        var ratesDict = new Dictionary<Currency, double>();
        foreach (var currency in CurrencyCodes.Keys)
        {
            if (CurrencyCodes[currency] != code)
            {
                if (rates.TryGetProperty(CurrencyCodes[currency], out var rateElement))
                {
                    ratesDict[currency] = rateElement.GetDouble();
                }
            }
        }

        var result = exchangeRate with { Rates = ratesDict };
        _cache?.Set(result);

        return result;
    }

    public double Convert(Currency from, Currency to, double amount)
    {
        throw new NotImplementedException("Use GetRatesAsync para obter as taxas e converter.");
    }
}
