using Spectre.Console;
using TrocaMoedas.Application.DTOs;
using TrocaMoedas.Application.Services;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Presentation.Commands;

public class ConvertCommand
{
    private readonly ConversionService _conversionService;

    public ConvertCommand(ConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    public async Task RunAsync()
    {
        try
        {
            var currencies = Enum.GetValues<Currency>().ToList();

            TerminalUi.ConfigureScreen();
            TerminalUi.ShowHeader("Início › Converter");
            TerminalUi.ShowStep(1, 3, "Moeda de origem");

            var from = InputPrompts.PromptCurrency("Selecione a moeda de origem", currencies);
            if (from is null)
            {
                ShowEof();
                return;
            }

            var destinations = InputPrompts.DestinationsFor(currencies, from.Value);

            TerminalUi.ConfigureScreen();
            TerminalUi.ShowHeader("Início › Converter");
            TerminalUi.ShowStep(2, 3, "Moeda de destino");

            var to = InputPrompts.PromptCurrency("Selecione a moeda de destino", destinations);
            if (to is null)
            {
                ShowEof();
                return;
            }

            TerminalUi.ConfigureScreen();
            TerminalUi.ShowHeader("Início › Converter");
            TerminalUi.ShowStep(3, 3, $"Valor em {from.Value}");

            var amount = InputPrompts.PromptAmount($"Valor em {from.Value} (ex.: 100 ou 100,50)");
            if (amount is null)
            {
                ShowEof();
                return;
            }

            var conversion = await _conversionService.ConvertAsync(from.Value, to.Value, amount.Value);

            ShowResult(conversion);
            TerminalUi.WaitForEnter();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            TerminalUi.WaitForEnter();
        }
    }

    private static void ShowResult(ConversionResult conversion)
    {
        var from = conversion.From;
        var to = conversion.To;

        var content = new Markup(
            $"[green bold]{to.GetSymbol()} {conversion.Result:N2} {to}[/]\n\n" +
            $"[grey]{from.GetSymbol()} {conversion.Amount:N2} {from}  →  1 {from} = {conversion.Rate:N4} {to}[/]\n" +
            $"[dim]{DateTime.Now:dd/MM/yyyy HH:mm}[/]")
        {
            Justification = Justify.Center
        };

        var panel = new Panel(content)
            .Header("[bold green] Conversão concluída [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green)
            .Padding(2, 1);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(panel);
    }

    private static void ShowEof() =>
        AnsiConsole.MarkupLine("[dim]Entrada desconectada (EOF). Encerrando.[/]");
}
