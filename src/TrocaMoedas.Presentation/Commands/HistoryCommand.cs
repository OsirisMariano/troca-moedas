using Spectre.Console;
using TrocaMoedas.Application.Repositories;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Presentation.Commands;

public class HistoryCommand
{
    private readonly IConversionRepository _conversionRepository;

    public HistoryCommand(IConversionRepository conversionRepository)
    {
        _conversionRepository = conversionRepository;
    }

    public async Task RunAsync()
    {
        try
        {
            TerminalUi.ConfigureScreen();
            TerminalUi.ShowHeader("Início › Histórico");

            var option = InputPrompts.PromptHistoryCount();
            if (option is null)
            {
                ShowEof();
                return;
            }

            var conversions = await _conversionRepository.GetRecentAsync(option.Count);

            TerminalUi.ConfigureScreen();
            TerminalUi.ShowHeader("Início › Histórico");

            if (conversions.Count == 0)
            {
                ShowEmptyState();
            }
            else
            {
                ShowHistoryTable(conversions);
                AnsiConsole.WriteLine();
                var label = conversions.Count == 1 ? "conversão" : "conversões";
                AnsiConsole.MarkupLine($"[dim]Mostrando [white]{conversions.Count}[/] {label}.[/]");
            }

            TerminalUi.WaitForEnter();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            TerminalUi.WaitForEnter();
        }
    }

    private static void ShowHistoryTable(IReadOnlyList<Conversion> conversions)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .Title("[bold]Histórico de Conversões[/]")
            .AddColumn(new TableColumn("[bold]#[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Data[/]"))
            .AddColumn(new TableColumn("[bold]Origem[/]"))
            .AddColumn(new TableColumn("[bold]Destino[/]"))
            .AddColumn(new TableColumn("[bold]Taxa[/]").RightAligned());

        for (var i = 0; i < conversions.Count; i++)
        {
            var conversion = conversions[i];

            table.AddRow(
                $"[dim]{i + 1}[/]",
                $"[dim]{conversion.CreatedAt:dd/MM HH:mm}[/]",
                $"[yellow]{conversion.FromCurrency.GetSymbol()} {conversion.Amount:N2} {conversion.FromCurrency}[/]",
                $"[green bold]{conversion.ToCurrency.GetSymbol()} {conversion.Result:N2}[/] [green]{conversion.ToCurrency}[/]",
                $"{conversion.Rate:N4}");
        }

        AnsiConsole.Write(table);
    }

    private static void ShowEmptyState()
    {
        var content = new Markup(
            "[dim]Nenhuma conversão registrada até agora.[/]\n\n" +
            "[dim]Faça sua primeira conversão na opção [white]1[/] do menu.[/]")
        {
            Justification = Justify.Center
        };

        var panel = new Panel(content)
            .Header("[bold yellow] Nenhuma conversão ainda [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Yellow)
            .Padding(2, 1);

        AnsiConsole.Write(panel);
    }

    private static void ShowEof() =>
        AnsiConsole.MarkupLine("[dim]Entrada desconectada (EOF). Encerrando.[/]");
}
