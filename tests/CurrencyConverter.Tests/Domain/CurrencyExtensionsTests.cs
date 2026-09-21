using Xunit;
using TrocaMoedas.Domain.Models;

namespace CurrencyConverter.Tests.Domain;

public class CurrencyExtensionsTests
{
    [Theory]
    [InlineData(Currency.BRL, "R$")]
    [InlineData(Currency.USD, "$")]
    [InlineData(Currency.EUR, "€")]
    [InlineData(Currency.GBP, "£")]
    [InlineData(Currency.JPY, "¥")]
    [InlineData(Currency.ARS, "$")]
    public void GetSymbol_ReturnsExpectedSymbol(Currency currency, string expected)
    {
        Assert.Equal(expected, currency.GetSymbol());
    }

    [Theory]
    [InlineData(Currency.BRL, "Real Brasileiro")]
    [InlineData(Currency.USD, "Dólar Americano")]
    [InlineData(Currency.EUR, "Euro")]
    [InlineData(Currency.GBP, "Libra Esterlina")]
    [InlineData(Currency.JPY, "Iene Japonês")]
    [InlineData(Currency.ARS, "Peso Argentino")]
    public void GetName_ReturnsExpectedName(Currency currency, string expected)
    {
        Assert.Equal(expected, currency.GetName());
    }

    [Fact]
    public void GetDisplayName_CombinesSymbolCodeAndName()
    {
        var currency = Currency.BRL;

        var display = currency.GetDisplayName();

        Assert.Equal("R$ BRL - Real Brasileiro", display);
    }
}