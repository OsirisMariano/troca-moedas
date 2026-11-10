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
}