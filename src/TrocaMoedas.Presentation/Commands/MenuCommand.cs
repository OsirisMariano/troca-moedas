using Spectre.Console;
using TrocaMoedas.Presentation.Commands;

namespace TrocaMoedas.Presentation;

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
                Console.Clear();

                AnsiConsole.Write(new FigletText("Conversor de Moedas").Color(Color.Blue));
                AnsiConsole.WriteLine();

                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Yellow)
                    .AddColumn(new TableColumn("[bold]#[/]").Centered())
                    .AddColumn(new TableColumn("[bold]Opção[/]").Centered())
                    .AddRow("[yellow]1[/]", "💱 Converter Moeda")
                    .AddRow("[yellow]2[/]", "📊 Histórico de Conversões")
                    .AddRow("[yellow]3[/]", "⚙️  Configurações")
                    .AddRow("[yellow]4[/]", "🚪 Sair");

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();

                AnsiConsole.MarkupLine("[bold yellow]Digite o número da opção:[/]");
                
                var input = ConsoleInput.ReadLine();
                if (ConsoleInput.IsEof(input))
                {
                    AnsiConsole.MarkupLine("[dim]Entrada desconectada (EOF). Encerrando.[/]");
                    return;
                }

                if (!int.TryParse(input, out var index) || index < 1 || index > 4)
                {
                    AnsiConsole.MarkupLine("[red]Opção inválida. Digite um número entre 1 e 4.[/]");
                    await Task.Delay(1500);
                    continue;
                }

                switch (index)
                {
                    case 1:
                        await _convertCommand.RunAsync();
                        break;
                    case 2:
                        await _historyCommand.RunAsync();
                        break;
                    case 3:
                        AnsiConsole.MarkupLine("[yellow]Funcionalidade será implementada no Passo 3[/]");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[dim]Pressione Enter para continuar...[/]");
                        Console.ReadLine();
                        break;
                    case 4:
                        AnsiConsole.MarkupLine("[green]Obrigado por usar o Conversor de Moedas![/]");
                        return;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
                AnsiConsole.MarkupLine("[dim]Pressione Enter para continuar...[/]");
                Console.ReadLine();
            }
        }
    }
}
