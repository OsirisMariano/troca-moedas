using TrocaMoedas.Domain.Models;
using TrocaMoedas.Presentation.Commands;
using Xunit;

namespace CurrencyConverter.Tests.Presentation;

public class InputPromptsTests
{
    [Fact]
    public void FormatCurrency_IncludesSymbolCodeAndName()
    {
        var formatted = InputPrompts.FormatCurrency(Currency.USD);

        Assert.Contains("$", formatted);
        Assert.Contains("USD", formatted);
        Assert.Contains("Dólar Americano", formatted);
    }

    [Theory]
    [InlineData(Currency.BRL, "R$", "Real Brasileiro")]
    [InlineData(Currency.EUR, "€", "Euro")]
    [InlineData(Currency.GBP, "£", "Libra Esterlina")]
    [InlineData(Currency.JPY, "¥", "Iene Japonês")]
    public void FormatCurrency_UsesCurrencyMetadata(Currency currency, string symbol, string name)
    {
        var formatted = InputPrompts.FormatCurrency(currency);

        Assert.Contains(symbol, formatted);
        Assert.Contains(name, formatted);
    }

    [Fact]
    public void HistoryCountOptions_HasExpectedLabelsAndCounts()
    {
        var options = InputPrompts.HistoryCountOptions;

        Assert.Collection(
            options,
            option => Assert.Equal((10, "10 conversões"), (option.Count, option.Label)),
            option => Assert.Equal((25, "25 conversões"), (option.Count, option.Label)),
            option => Assert.Equal((50, "50 conversões"), (option.Count, option.Label)),
            option => Assert.Equal(int.MaxValue, option.Count));
    }

    [Theory]
    [InlineData(Currency.BRL)]
    [InlineData(Currency.USD)]
    [InlineData(Currency.ARS)]
    public void DestinationsFor_RemovesOriginCurrency(Currency origin)
    {
        var all = Enum.GetValues<Currency>();

        var destinations = InputPrompts.DestinationsFor(all, origin);

        Assert.DoesNotContain(origin, destinations);
        Assert.Equal(all.Length - 1, destinations.Count);
    }

    [Fact]
    public void DestinationsFor_KeepsEnumOrder()
    {
        var all = Enum.GetValues<Currency>();

        var destinations = InputPrompts.DestinationsFor(all, Currency.BRL);

        Assert.Equal(
            new[] { Currency.USD, Currency.EUR, Currency.GBP, Currency.JPY, Currency.ARS },
            destinations);
    }
}
