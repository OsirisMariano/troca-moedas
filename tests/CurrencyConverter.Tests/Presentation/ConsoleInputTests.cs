using TrocaMoedas.Presentation;
using Xunit;

namespace CurrencyConverter.Tests.Presentation;

public class ConsoleInputTests
{
    [Fact]
    public void IsEof_NullInput_ReturnsTrue()
    {
        Assert.True(ConsoleInput.IsEof(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("   ")]
    [InlineData("BRL")]
    public void IsEof_NonNullInput_ReturnsFalse(string input)
    {
        Assert.False(ConsoleInput.IsEof(input));
    }

    [Theory]
    [InlineData("100", 100.0)]
    [InlineData("100.50", 100.5)]
    [InlineData("100,50", 100.5)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("1,234.56", 1234.56)]
    [InlineData("  42  ", 42.0)]
    public void TryParseAmount_ValidInputs_ReturnsTrue(string input, double expected)
    {
        Assert.True(ConsoleInput.TryParseAmount(input, out var amount));
        Assert.Equal(expected, amount, precision: 2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("1.2.3")]
    [InlineData("10,20,30")]
    public void TryParseAmount_InvalidInputs_ReturnsFalse(string? input)
    {
        Assert.False(ConsoleInput.TryParseAmount(input, out var amount));
        Assert.Equal(0, amount);
    }
}
