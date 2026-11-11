using System.Net;
using System.Text.Json;
using TrocaMoedas.Application.Services;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Infrastructure.Services;

public class ExchangeRateApiService : IExchangeRateService
{
    public const int DefaultMaxAttempts = 3;

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IExchangeRateCache? _cache;
    private readonly Func<TimeSpan, Task> _delay;
    private readonly int _maxAttempts;
    private readonly Action<string>? _log;

    private static readonly Dictionary<Currency, string> CurrencyCodes = new()
    {
        [Currency.BRL] = "BRL",
        [Currency.USD] = "USD",
        [Currency.EUR] = "EUR",
        [Currency.GBP] = "GBP",
        [Currency.JPY] = "JPY",
        [Currency.ARS] = "ARS"
    };

    public ExchangeRateApiService(
        HttpClient httpClient,
        string apiKey,
        IExchangeRateCache? cache = null,
        Func<TimeSpan, Task>? delay = null,
        int maxAttempts = DefaultMaxAttempts,
        Action<string>? log = null)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "É necessária pelo menos uma tentativa.");
        }

        _httpClient = httpClient;
        _apiKey = apiKey;
        _cache = cache;
        _delay = delay ?? Task.Delay;
        _maxAttempts = maxAttempts;
        _log = log;
    }

    public async Task<ExchangeRate> GetRatesAsync(Currency baseCurrency)
    {
        if (_cache is not null && _cache.TryGet(baseCurrency, out var cached))
        {
            return cached;
        }

        var code = CurrencyCodes[baseCurrency];
        var url = $"https://v6.exchangerate-api.com/v6/{_apiKey}/latest/{code}";

        for (var attempt = 1; ; attempt++)
        {
            using var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await ParseAndCacheAsync(response, baseCurrency, code);
            }

            if (!IsRetryable(response.StatusCode) || attempt >= _maxAttempts)
            {
                response.EnsureSuccessStatusCode();
            }

            var wait = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
            _log?.Invoke(
                $"API retornou {(int)response.StatusCode}; tentativa {attempt}/{_maxAttempts}. " +
                $"Aguardando {wait.TotalSeconds:0}s...");
            await _delay(wait);
        }
    }

    public double Convert(Currency from, Currency to, double amount)
    {
        throw new NotImplementedException("Use GetRatesAsync para obter as taxas e converter.");
    }

    private async Task<ExchangeRate> ParseAndCacheAsync(
        HttpResponseMessage response,
        Currency baseCurrency,
        string code)
    {
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
            if (CurrencyCodes[currency] != code
                && rates.TryGetProperty(CurrencyCodes[currency], out var rateElement))
            {
                ratesDict[currency] = rateElement.GetDouble();
            }
        }

        var result = exchangeRate with { Rates = ratesDict };
        _cache?.Set(result);

        return result;
    }

    private static bool IsRetryable(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
}
