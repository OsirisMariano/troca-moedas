using TrocaMoedas.Application.DTOs;
using TrocaMoedas.Application.Repositories;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Application.Services;

public class ConversionService
{
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IConversionRepository _conversionRepository;

    public ConversionService(IExchangeRateService exchangeRateService, IConversionRepository conversionRepository)
    {
        _exchangeRateService = exchangeRateService;
        _conversionRepository = conversionRepository;
    }

    public async Task<ConversionResult> ConvertAsync(Currency from, Currency to, double amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor deve ser maior que zero.");
        }

        var exchangeRate = await _exchangeRateService.GetRatesAsync(from);
        var rate = exchangeRate.GetRate(from, to);
        var result = amount * rate;

        var conversion = new Conversion
        {
            FromCurrency = from,
            ToCurrency = to,
            Amount = amount,
            Result = result,
            Rate = rate
        };

        await _conversionRepository.SaveAsync(conversion);

        return new ConversionResult(from, to, amount, result, rate);
    }
}