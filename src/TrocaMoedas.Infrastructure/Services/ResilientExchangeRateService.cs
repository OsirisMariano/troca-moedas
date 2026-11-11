using System.Text.Json;
using TrocaMoedas.Application.Services;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Infrastructure.Services;

/// <summary>
/// Usa o serviço principal (API) e cai para o fallback quando a API fica
/// indisponível, atinge o limite de requisições ou retorna erro.
/// </summary>
public sealed class ResilientExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateService _primary;
    private readonly IExchangeRateService _fallback;
    private readonly Action<string>? _log;

    public ResilientExchangeRateService(
        IExchangeRateService primary,
        IExchangeRateService fallback,
        Action<string>? log = null)
    {
        _primary = primary;
        _fallback = fallback;
        _log = log;
    }

    public async Task<ExchangeRate> GetRatesAsync(Currency baseCurrency)
    {
        try
        {
            return await _primary.GetRatesAsync(baseCurrency);
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            _log?.Invoke($"API indisponível: {ex.Message} Usando taxas de fallback.");
            return await _fallback.GetRatesAsync(baseCurrency);
        }
    }

    public double Convert(Currency from, Currency to, double amount) =>
        _fallback.Convert(from, to, amount);

    private static bool IsRecoverable(Exception exception) =>
        exception is HttpRequestException
            or TaskCanceledException
            or IOException
            or JsonException
            or KeyNotFoundException;
}
