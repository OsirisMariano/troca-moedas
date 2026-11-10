using Moq;
using TrocaMoedas.Application.Repositories;
using TrocaMoedas.Application.Services;
using TrocaMoedas.Domain.Models;
using Xunit;

namespace CurrencyConverter.Tests.Services;

public class ConversionServiceTests
{
    [Fact]
    public async Task ConvertAsync_OnSuccess_ComputesResultAndSavesConversion()
    {
        var exchangeRateService = new Mock<IExchangeRateService>();
        exchangeRateService
            .Setup(s => s.GetRatesAsync(Currency.BRL))
            .ReturnsAsync(new ExchangeRate
            {
                BaseCurrency = Currency.BRL,
                Rates = new Dictionary<Currency, double> { [Currency.USD] = 0.2 }
            });

        var repository = new Mock<IConversionRepository>();
        var service = new ConversionService(exchangeRateService.Object, repository.Object);

        var result = await service.ConvertAsync(Currency.BRL, Currency.USD, 100);

        Assert.Equal(Currency.BRL, result.From);
        Assert.Equal(Currency.USD, result.To);
        Assert.Equal(100, result.Amount);
        Assert.Equal(20, result.Result, precision: 6);
        Assert.Equal(0.2, result.Rate, precision: 6);

        repository.Verify(r => r.SaveAsync(It.Is<Conversion>(c =>
            c.FromCurrency == Currency.BRL &&
            c.ToCurrency == Currency.USD &&
            c.Amount == 100 &&
            c.Result == 20 &&
            c.Rate == 0.2)), Times.Once);
    }

    [Fact]
    public async Task ConvertAsync_WhenAmountIsNotPositive_ThrowsAndDoesNotTouchRepositories()
    {
        var exchangeRateService = new Mock<IExchangeRateService>();
        var repository = new Mock<IConversionRepository>();
        var service = new ConversionService(exchangeRateService.Object, repository.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.ConvertAsync(Currency.BRL, Currency.USD, 0));

        exchangeRateService.Verify(s => s.GetRatesAsync(It.IsAny<Currency>()), Times.Never);
        repository.Verify(r => r.SaveAsync(It.IsAny<Conversion>()), Times.Never);
    }

    [Fact]
    public async Task ConvertAsync_WhenRateServiceFails_PropagatesException()
    {
        var exchangeRateService = new Mock<IExchangeRateService>();
        exchangeRateService
            .Setup(s => s.GetRatesAsync(It.IsAny<Currency>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        var repository = new Mock<IConversionRepository>();
        var service = new ConversionService(exchangeRateService.Object, repository.Object);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.ConvertAsync(Currency.USD, Currency.BRL, 10));

        repository.Verify(r => r.SaveAsync(It.IsAny<Conversion>()), Times.Never);
    }

    [Fact]
    public async Task ConvertAsync_CrossCurrency_UsesRateFromRateService()
    {
        var exchangeRateService = new Mock<IExchangeRateService>();
        exchangeRateService
            .Setup(s => s.GetRatesAsync(Currency.USD))
            .ReturnsAsync(new ExchangeRate
            {
                BaseCurrency = Currency.USD,
                Rates = new Dictionary<Currency, double>
                {
                    [Currency.BRL] = 5.0,
                    [Currency.EUR] = 0.926
                }
            });

        var repository = new Mock<IConversionRepository>();
        var service = new ConversionService(exchangeRateService.Object, repository.Object);

        var result = await service.ConvertAsync(Currency.USD, Currency.EUR, 10);

        Assert.Equal(9.26, result.Result, precision: 6);
    }
}