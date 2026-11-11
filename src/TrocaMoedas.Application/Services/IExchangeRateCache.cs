using System.Diagnostics.CodeAnalysis;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Application.Services;

public interface IExchangeRateCache
{
    bool TryGet(Currency baseCurrency, [NotNullWhen(true)] out ExchangeRate? rate);
    void Set(ExchangeRate rate);
}
