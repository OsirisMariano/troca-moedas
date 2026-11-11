using TrocaMoedas.Presentation.Commands;
using Xunit;

namespace CurrencyConverter.Tests.Presentation;

public class HistoryCommandTests
{
    [Theory]
    [InlineData("", HistoryAction.Back)]
    [InlineData("1", HistoryAction.Back)]
    [InlineData("voltar", HistoryAction.Back)]
    [InlineData(" 2 ", HistoryAction.Clear)]
    [InlineData("limpar", HistoryAction.Clear)]
    [InlineData("LIMPAR", HistoryAction.Clear)]
    public void TryParseAction_ValidInput_ReturnsTrue(string input, HistoryAction expected)
    {
        Assert.True(HistoryCommand.TryParseAction(input, out var action));
        Assert.Equal(expected, action);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("3")]
    [InlineData("abc")]
    public void TryParseAction_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(HistoryCommand.TryParseAction(input, out _));
    }

    [Theory]
    [InlineData("s")]
    [InlineData("S")]
    [InlineData("sim")]
    [InlineData("y")]
    [InlineData("yes")]
    [InlineData("1")]
    [InlineData("  s  ")]
    public void TryParseConfirmation_AffirmativeInput_ReturnsTrue(string input)
    {
        Assert.True(HistoryCommand.TryParseConfirmation(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("n")]
    [InlineData("nao")]
    [InlineData("não")]
    [InlineData("0")]
    [InlineData("abc")]
    public void TryParseConfirmation_NonAffirmativeInput_ReturnsFalse(string? input)
    {
        Assert.False(HistoryCommand.TryParseConfirmation(input));
    }
}
