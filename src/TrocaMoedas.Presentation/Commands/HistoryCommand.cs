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
            Console.Clear();

            AnsiConsole.MarkupLine("[bold blue]📊 Histórico de Conversões[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[bold yellow]Quantas conversões deseja ver? (padrão 10):[/]");
            var input = ConsoleInput.ReadLine();
            if (ConsoleInput.IsEof(input))
            {
                AnsiConsole.MarkupLine("[dim]Entrada desconectada (EOF). Encerrando.[/]");
                return;
            }
            
            var count = 10;
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out var parsed))
            {
                count = parsed >= 1 && parsed <= 100 ? parsed : 10;
            }

            var conversions = await _conversionRepository.GetRecentAsync(count);

            if (!conversions.Any())
            {
                AnsiConsole.MarkupLine("[dim]Nenhuma conversão encontrada.[/]");
            }
            else
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Blue)
                    .Title("[bold]Histórico de Conversões[/]")
                    .AddColumn("[bold]Data[/]")
                    .AddColumn("[bold]Origem[/]")
                    .AddColumn("[bold]Valor[/]")
                    .AddColumn("[bold]Destino[/]")
                    .AddColumn("[bold]Resultado[/]")
                    .AddColumn("[bold]Taxa[/]");

                foreach (var c in conversions)
                {
                    table.AddRow(
                        c.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        $"{c.FromCurrency.GetSymbol()} {c.FromCurrency}",
                        $"{c.Amount:N2}",
                        $"{c.ToCurrency.GetSymbol()} {c.ToCurrency}",
                        $"{c.Result:N2}",
                        $"{c.Rate:N4}"
                    );
                }

                AnsiConsole.Write(table);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Pressione Enter para continuar...[/]");
            if (ConsoleInput.IsEof(ConsoleInput.ReadLine()))
            {
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
