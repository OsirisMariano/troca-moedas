using TrocaMoedas.Application.Services;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Infrastructure.Services;

public class FallbackExchangeRateService : IExchangeRateService
{
    private static readonly Dictionary<Currency, double> FallbackRates = new()
    {
        [Currency.USD] = 5.00,
        [Currency.EUR] = 5.40,
        [Currency.GBP] = 6.30,
        [Currency.JPY] = 0.034,
        [Currency.ARS] = 0.005
    };

    public Task<ExchangeRate> GetRatesAsync(Currency baseCurrency)
    {
        var rates = new Dictionary<Currency, double>();

        if (baseCurrency == Currency.BRL)
        {
            foreach (var currency in FallbackRates.Keys)
            {
                rates[currency] = 1.0 / FallbackRates[currency];
            }
        }
        else
        {
            var brlPerBase = FallbackRates.TryGetValue(baseCurrency, out var rate) ? rate : 1.0;

            rates[Currency.BRL] = brlPerBase;

            foreach (var currency in FallbackRates.Keys)
            {
                if (currency != baseCurrency)
                {
                    rates[currency] = brlPerBase / FallbackRates[currency];
                }
            }
        }

        var exchangeRate = new ExchangeRate
        {
            BaseCurrency = baseCurrency,
            Rates = rates,
            Timestamp = DateTime.UtcNow
        };

        return Task.FromResult(exchangeRate);
    }

    public double Convert(Currency from, Currency to, double amount)
    {
        if (from == to) return amount;

        var fromRate = FallbackRates.GetValueOrDefault(from, 1.0);
        var toRate = FallbackRates.GetValueOrDefault(to, 1.0);

        var amountInBrl = from == Currency.BRL ? amount : amount * fromRate;

        return to == Currency.BRL ? amountInBrl : amountInBrl / toRate;
    }
}
