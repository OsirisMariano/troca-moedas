using Spectre.Console;

namespace TrocaMoedas.Presentation.Commands;

public class MenuCommand
{
    private readonly ConvertCommand _convertCommand;
    private readonly HistoryCommand _historyCommand;

    public MenuCommand(ConvertCommand convertCommand, HistoryCommand historyCommand)
    {
        _convertCommand = convertCommand;
        _historyCommand = historyCommand;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            try
            {
                TerminalUi.ConfigureScreen();

                TerminalUi.ShowHeader("Ambiente: Docker / SQLite");

                var choice = PromptChoice();
                if (choice is null)
                {
                    AnsiConsole.MarkupLine("[dim]Entrada desconectada (EOF). Encerrando.[/]");
                    return;
                }

                AnsiConsole.Clear();

                switch (choice.Value)
                {
                    case MenuChoice.Convert:
                        await _convertCommand.RunAsync();
                        break;
                    case MenuChoice.History:
                        await _historyCommand.RunAsync();
                        break;
                    case MenuChoice.Settings:
                        if (!ShowSettingsPlaceholder())
                        {
                            return;
                        }
                        break;
                    case MenuChoice.Exit:
                        AnsiConsole.MarkupLine("[red bold]Encerrando aplicação. Até logo![/]");
                        return;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
                AnsiConsole.MarkupLine("[dim]Pressione Enter para continuar...[/]");
                if (ConsoleInput.IsEof(ConsoleInput.ReadLine()))
                {
                    return;
                }
            }
        }
    }

    public static bool TryParseChoice(string? input, out MenuChoice choice)
    {
        switch (input?.Trim())
        {
            case "1":
                choice = MenuChoice.Convert;
                return true;
            case "2":
                choice = MenuChoice.History;
                return true;
            case "3":
                choice = MenuChoice.Settings;
                return true;
            case "4":
                choice = MenuChoice.Exit;
                return true;
            default:
                choice = default;
                return false;
        }
    }

    private static MenuChoice? PromptChoice()
    {
        if (Console.IsInputRedirected)
        {
            while (true)
            {
                AnsiConsole.MarkupLine("[bold yellow]Digite o número da opção:[/]");
                var input = ConsoleInput.ReadLine();
                if (ConsoleInput.IsEof(input))
                {
                    return null;
                }

                if (TryParseChoice(input, out var choice))
                {
                    return choice;
                }

                AnsiConsole.MarkupLine("[red]Opção inválida. Digite um número entre 1 e 4.[/]");
            }
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<MenuChoice>()
                .Title("[yellow]Use as setas para navegar e pressione [green]Enter[/]:[/]")
                .PageSize(4)
                .UseConverter(FormatChoice)
                .AddChoices(Enum.GetValues<MenuChoice>()));
    }

    private static string FormatChoice(MenuChoice choice) => choice switch
    {
        MenuChoice.Convert => "[green]1. Converter Moeda[/]",
        MenuChoice.History => "[blue]2. Histórico de Conversões[/]",
        MenuChoice.Settings => "[blue]3. Configurações[/]",
        MenuChoice.Exit => "[red]4. Sair[/]",
        _ => choice.ToString()
    };

    private static bool ShowSettingsPlaceholder()
    {
        AnsiConsole.MarkupLine("[cyan]⚙️ Módulo de Configurações em breve![/]");
        AnsiConsole.MarkupLine("[dim]Pressione Enter para voltar ao menu...[/]");
        return !ConsoleInput.IsEof(ConsoleInput.ReadLine());
    }
}
