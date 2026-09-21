using Spectre.Console;
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
            Console.Clear();

            AnsiConsole.MarkupLine("[bold blue]💱 Conversão de Moedas[/]");
            AnsiConsole.WriteLine();

            var currencies = Enum.GetValues<Currency>().ToList();

            var fromTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title("[bold]Moeda de Origem[/]")
                .AddColumn(new TableColumn("[bold]#[/]").Centered())
                .AddColumn(new TableColumn("[bold]Moeda[/]").Centered())
                .AddColumn(new TableColumn("[bold]Nome[/]").Centered());

            for (int i = 0; i < currencies.Count; i++)
            {
                var c = currencies[i];
                fromTable.AddRow($"[yellow]{i + 1}[/]", $"{c.GetSymbol()} {c}", c.GetName());
            }

            AnsiConsole.Write(fromTable);
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[bold yellow]Digite o número da moeda de origem:[/]");
            var fromInput = ConsoleInput.ReadLine();
            if (ConsoleInput.IsEof(fromInput))
            {
                AnsiConsole.MarkupLine("[dim]Entrada desconectada (EOF). Encerrando.[/]");
                return;
            }

            if (!int.TryParse(fromInput, out var fromIndex) || fromIndex < 1 || fromIndex > currencies.Count)
            {
                AnsiConsole.MarkupLine("[red]Opção inválida. Tente novamente.[/]");
                await Task.Delay(1500);
                return;
            }

            Console.Clear();

            AnsiConsole.MarkupLine("[bold blue]💱 Conversão de Moedas[/]");
            AnsiConsole.WriteLine();

            var toTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title("[bold]Moeda de Destino[/]")
                .AddColumn(new TableColumn("[bold]#[/]").Centered())
                .AddColumn(new TableColumn("[bold]Moeda[/]").Centered())
                .AddColumn(new TableColumn("[bold]Nome[/]").Centered());

            for (int i = 0; i < currencies.Count; i++)
            {
                var c = currencies[i];
                toTable.AddRow($"[yellow]{i + 1}[/]", $"{c.GetSymbol()} {c}", c.GetName());
            }

            AnsiConsole.Write(toTable);
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[bold yellow]Digite o número da moeda de destino:[/]");
            var toInput = ConsoleInput.ReadLine();
            if (ConsoleInput.IsEof(toInput))
            {
                AnsiConsole.MarkupLine("[dim]Entrada desconectada (EOF). Encerrando.[/]");
                return;
            }

            if (!int.TryParse(toInput, out var toIndex) || toIndex < 1 || toIndex > currencies.Count)
            {
                AnsiConsole.MarkupLine("[red]Opção inválida. Tente novamente.[/]");
                await Task.Delay(1500);
                return;
            }

            AnsiConsole.MarkupLine("[bold yellow]Digite o valor:[/]");
            var amountInput = ConsoleInput.ReadLine();
            if (ConsoleInput.IsEof(amountInput))
            {
                AnsiConsole.MarkupLine("[dim]Entrada desconectada (EOF). Encerrando.[/]");
                return;
            }

            if (!double.TryParse(amountInput, out var amount) || amount <= 0)
            {
                AnsiConsole.MarkupLine("[red]O valor deve ser maior que zero.[/]");
                await Task.Delay(1500);
                return;
            }

            var from = currencies[fromIndex - 1];
            var to = currencies[toIndex - 1];

            var conversion = await _conversionService.ConvertAsync(from, to, amount);

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title("[bold green]Resultado[/]")
                .AddColumn(new TableColumn("[bold]Detalhe[/]").Centered())
                .AddColumn(new TableColumn("[bold]Valor[/]").Centered())
                .AddRow("Moeda de Origem", $"{from.GetSymbol()} {conversion.Amount:N2} {from}")
                .AddRow("Moeda de Destino", $"{to.GetSymbol()} {conversion.Result:N2} {to}")
                .AddRow("Taxa de Câmbio", $"1 {from} = {conversion.Rate:N4} {to}")
                .AddRow("Data/Hora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Pressione Enter para continuar...[/]");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            AnsiConsole.MarkupLine("[dim]Pressione Enter para continuar...[/]");
            Console.ReadLine();
        }
    }
}
