using TrocaMoedas.Presentation.Commands;
using Xunit;

namespace CurrencyConverter.Tests.Presentation;

public class MenuCommandTests
{
    [Theory]
    [InlineData("1", MenuChoice.Convert)]
    [InlineData("2", MenuChoice.History)]
    [InlineData("3", MenuChoice.Settings)]
    [InlineData("4", MenuChoice.Exit)]
    [InlineData(" 2 ", MenuChoice.History)]
    public void TryParseChoice_ValidInput_ReturnsTrue(string input, MenuChoice expected)
    {
        Assert.True(MenuCommand.TryParseChoice(input, out var choice));
        Assert.Equal(expected, choice);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0")]
    [InlineData("5")]
    [InlineData("abc")]
    public void TryParseChoice_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(MenuCommand.TryParseChoice(input, out _));
    }
}
