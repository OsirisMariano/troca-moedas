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

            var action = PromptAction();
            if (action is null)
            {
                ShowEof();
                return;
            }

            if (action == HistoryAction.Clear)
            {
                if (!await ClearHistoryAsync())
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            TerminalUi.WaitForEnter();
        }
    }

    private async Task<bool> ClearHistoryAsync()
    {
        var confirmation = PromptConfirmation("Tem certeza que deseja apagar todo o histórico?");
        if (confirmation is null)
        {
            ShowEof();
            return false;
        }

        if (!confirmation.Value)
        {
            AnsiConsole.MarkupLine("[yellow]Operação cancelada.[/]");
            TerminalUi.WaitForEnter();
            return true;
        }

        await _conversionRepository.ClearAsync();
        AnsiConsole.MarkupLine("[green]✔ Histórico limpo com sucesso.[/]");
        TerminalUi.WaitForEnter();
        return true;
    }

    private static HistoryAction? PromptAction()
    {
        if (Console.IsInputRedirected)
        {
            while (true)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[bold yellow]O que deseja fazer? (Enter=Voltar, 2=Limpar histórico)[/]");

                var input = ConsoleInput.ReadLine();
                if (ConsoleInput.IsEof(input))
                {
                    return null;
                }

                if (TryParseAction(input, out var action))
                {
                    return action;
                }

                AnsiConsole.MarkupLine("[red]Opção inválida. Digite 1 para voltar ou 2 para limpar.[/]");
            }
        }

        AnsiConsole.WriteLine();
        return AnsiConsole.Prompt(
            new SelectionPrompt<HistoryAction>()
                .Title("[yellow]O que deseja fazer?[/]")
                .UseConverter(action => action == HistoryAction.Clear
                    ? "[red]Limpar histórico[/]"
                    : "[green]Voltar ao menu[/]")
                .AddChoices(HistoryAction.Back, HistoryAction.Clear));
    }

    private static bool? PromptConfirmation(string title)
    {
        if (Console.IsInputRedirected)
        {
            AnsiConsole.MarkupLine($"[bold yellow]{title} (s/n)[/]");

            var input = ConsoleInput.ReadLine();
            return ConsoleInput.IsEof(input) ? null : TryParseConfirmation(input);
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title($"[yellow]{title}[/]")
                .UseConverter(value => value ? "[red]Sim, apagar[/]" : "[green]Não, cancelar[/]")
                .AddChoices(false, true));
    }

    public static bool TryParseAction(string? input, out HistoryAction action)
    {
        switch (input?.Trim().ToLowerInvariant())
        {
            case "":
            case "1":
            case "voltar":
                action = HistoryAction.Back;
                return true;
            case "2":
            case "limpar":
                action = HistoryAction.Clear;
                return true;
            default:
                action = default;
                return false;
        }
    }

    public static bool TryParseConfirmation(string? input) =>
        input?.Trim().ToLowerInvariant() is "s" or "sim" or "y" or "yes" or "1";

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
