using TrocaMoedas.Presentation;
using Xunit;

namespace CurrencyConverter.Tests.Presentation;

public class TerminalUiTests
{
    [Theory]
    [InlineData(0, TerminalUi.DefaultWidth)]
    [InlineData(-1, TerminalUi.DefaultWidth)]
    [InlineData(77, 77)]
    [InlineData(120, 120)]
    public void ResolveWidth_UsesFallbackWhenConsoleWidthIsInvalid(int consoleWidth, int expected)
    {
        Assert.Equal(expected, TerminalUi.ResolveWidth(consoleWidth));
    }
}
